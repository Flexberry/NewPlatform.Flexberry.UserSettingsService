namespace ICSSoft.Services
{
    using System;
    using System.Collections.Generic;
    using System.DirectoryServices;
    using System.Linq;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.FunctionalLanguage;
    using ICSSoft.STORMNET.FunctionalLanguage.SQLWhere;

    /// <summary>
    /// Implementation of <see cref="IUserSettingsService"/> using data service
    /// for loading and saving settings.
    /// </summary>
    public class UserSettingsService : DataServiceWrapper, IUserSettingsService
    {
        /// <summary>
        /// Object for syncronizing access to current service instance in <see cref="Current"/>.
        /// </summary>
        private static readonly object _sync = new object();

        /// <summary>
        /// Current instance of the service.
        /// </summary>
        private static IUserSettingsService _current;

        /// <summary>
        /// Identifier of module for common settings.
        /// </summary>
        private readonly Guid _commonModuleGuid = new Guid("00000000-0000-0000-0000-000000000000");

        /// <summary>
        /// Identifier of user for common settings.
        /// </summary>
        private readonly Guid _commonUserGuid = new Guid("00000000-0000-0000-0000-000000000000");

        /// <summary>
        /// Flag of enabled settings.
        /// </summary>
        private bool? _useSettings;

        /// <summary>
        /// Name of the application.
        /// </summary>
        private string _appName;

        /// <summary>
        /// Current instcance of settings service.
        /// </summary>
        /// <remarks>
        /// By default, without any configuration (for example, by Unity or other IoC containers) that is usually made
        /// on start of the application, instance of <see cref="UserSettingsService"/> with data service from
        /// <see cref="DataServiceProvider"/> will be created.
        /// Thread safe.
        /// </remarks>
        public static IUserSettingsService Current
        {
            get
            {
                if (_current == null)
                { 
                    lock (_sync)
                    {
                        if (_current == null)
                            _current = new UserSettingsService();
                    }
                }

                return _current;
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                lock (_sync)
                    _current = value;
            }
        }

        /// <summary>
        /// Default constructor.
        /// Uses data service from <see cref="DataServiceProvider"/>.
        /// </summary>
        public UserSettingsService()
        { 
        }

        /// <summary>
        /// Constructor with parameters.
        /// Uses specified data service for loading and saving settings.
        /// </summary>
        /// <param name="dataService">Data service for working with settings.</param>
        public UserSettingsService(IDataService dataService) : base(dataService)
        {
        }

        #region Configuration

        /// <summary>
        /// Flag of enabled service.
        /// </summary>
        public bool UseSettings
        {
            get
            {
                if (_useSettings != null)
                    return _useSettings.Value;

                string configurationValue = System.Configuration.ConfigurationManager.AppSettings["UseSettings"];
                _useSettings = configurationValue.ToLower() == "true";

                return _useSettings.Value;
            }

            set
            {
                _useSettings = value;
            }
        }

        /// <summary>
        /// Name of application for loading data.
        /// </summary>
        public string AppName
        {
            get
            {
                if (_appName != null)
                    return _appName;

                _appName = System.Configuration.ConfigurationManager.AppSettings["applicationName"];
                if (string.IsNullOrEmpty(_appName))
                    _appName = System.IO.Path.GetFileNameWithoutExtension(System.Windows.Forms.Application.ExecutablePath);

                return _appName;
            }
        }

        /// <summary>
        /// Name of module that is used for common settings.
        /// </summary>
        public string CommonModuleName
        {
            get { return "+"; }
        }

        /// <summary>
        /// Name of user that is used for common settings.
        /// </summary>
        public string CommonUserName
        {
            get { return "+"; }
        }

        /// <summary>
        /// Identifier of module that is used for common settings.
        /// </summary>
        public Guid CommonModuleGuid
        {
            get { return _commonModuleGuid; }
        }

        /// <summary>
        /// Identifier of user that is used for common settings.
        /// </summary>
        public Guid CommonUserGuid
        {
            get { return _commonUserGuid; }
        }

        #endregion

        /// <summary>
        /// Get the name of user from AD.
        /// </summary>
        /// <param name="userName">Name of user for search in AD.</param>
        /// <returns>Name of user from AD or <c>null</c>, if it's not found.</returns>
        public string GetADUserName(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                return null;

            string retUserName = null;
            try
            {
                var ds = new DirectorySearcher("(&(objectClass=user)(sAMAccountName= " + userName + "))", new[] { "cn" })
                {
                    CacheResults = true
                };

                SearchResult sr = ds.FindOne();
                if (sr != null && sr.Properties != null && sr.Properties["cn"] != null && sr.Properties["cn"].Count > 0)
                {
                    retUserName = sr.Properties["cn"][0].ToString();
                }
            }
            catch (Exception e)
            {
                LogService.LogError("Exception in loading AD user name.", e);
            }
            
            return retUserName;
        }

        /// <summary>
        /// Get the name of current user (using <see cref="Environment.UserName" />) from AD.
        /// </summary>
        /// <returns>Name of current user from AD or <see cref="Environment.UserName" />, if it's not found.</returns>
        public string GetEnvOrADUserName()
        {
            return GetADUserName(Environment.UserName) ?? Environment.UserName;
        }

        #region Loading

        /// <summary>
        /// Load setting values by names and identifiers of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="stringValue">Value of <see cref="string" />-type setting with bounded length.</param>
        /// <param name="textValue">Value of <see cref="string" />-type setting with unbounded length.</param>
        /// <param name="intValue">Value of <see cref="int" />-type setting.</param>
        /// <param name="boolValue">Value of <see cref="bool" />-type setting.</param>
        /// <param name="guidValue">Value of <see cref="Guid" />-type setting.</param>
        /// <param name="decimalValue">Value of <see cref="decimal" />-type setting.</param>
        /// <param name="dateTimeValue">Value of <see cref="DateTime" />-type setting.</param>
        /// <returns>Returns <c>true</c> if data successfully loaded and <c>flase</c> if loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>
        /// <para>Loading logic: (user name OR identifier) AND (module name OR identifier) AND (setting name OR identifier).</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        public bool GetSettings(string userName, Guid? userGuid, string moduleName,  Guid? moduleGuid,  string settingName,  Guid? settingGuid,  out string stringValue,  out string textValue,  out int? intValue,  out bool? boolValue,  out Guid? guidValue,  out decimal? decimalValue,  out DateTime? dateTimeValue)
        {
            stringValue = null;
            textValue = null;
            intValue = null;
            boolValue = null;
            guidValue = null;
            decimalValue = null;
            dateTimeValue = null;

            if (!UseSettings)
                return false;

            if (CheckSettingIdentityIsNull(userName, userGuid, moduleName, moduleGuid, settingName, settingGuid))
                throw new ArgumentException();

            try
            {
                DataObject[] dObjs = GetObectsWithPropsInVeiw(
                    userName, 
                    userGuid, 
                    moduleName,
                    moduleGuid, 
                    settingName, 
                    settingGuid, 
                    new[] { "StrVal", "TxtVal", "IntVal", "BoolVal", "GuidVal", "DecimalVal", "DateTimeVal" });

                if (dObjs.Length > 0)
                {
                    DeleteObjectsFrom1ToN(DataService, dObjs);

                    UserSetting userSett = (UserSetting)dObjs[0];
                    stringValue = userSett.StrVal;
                    textValue = userSett.TxtVal;
                    intValue = userSett.IntVal;
                    boolValue = userSett.BoolVal;
                    guidValue = userSett.GuidVal;
                    decimalValue = userSett.DecimalVal;
                    dateTimeValue = userSett.DateTimeVal;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogService.LogError("Exception in loading user settings.", ex);
                throw;
            }
        }

        /// <summary>
        /// Load setting values by names of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="stringValue">Value of <see cref="string" />-type setting with bounded length.</param>
        /// <param name="textValue">Value of <see cref="string" />-type setting with unbounded length.</param>
        /// <param name="intValue">Value of <see cref="int" />-type setting.</param>
        /// <param name="boolValue">Value of <see cref="bool" />-type setting.</param>
        /// <param name="guidValue">Value of <see cref="Guid" />-type setting.</param>
        /// <param name="decimalValue">Value of <see cref="decimal" />-type setting.</param>
        /// <param name="dateTimeValue">Value of <see cref="DateTime" />-type setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <returns>Returns <c>true</c> if data successfully loaded and <c>flase</c> if loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>
        /// <para>Loading logic: user name AND module name AND setting name.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        public bool GetSettings(
            string userName,
            string moduleName,
            string settingName,
            out string stringValue,
            out string textValue,
            out int? intValue,
            out bool? boolValue,
            out Guid? guidValue,
            out decimal? decimalValue,
            out DateTime? dateTimeValue)
        {
            return GetSettings(
                userName,
                null,
                moduleName,
                null,
                settingName,
                null,
                out stringValue,
                out textValue,
                out intValue,
                out boolValue,
                out guidValue,
                out decimalValue,
                out dateTimeValue);
        }

        /// <summary>
        /// Load setting values by identifiers of user and module.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="stringValue">Value of <see cref="string" />-type setting with bounded length.</param>
        /// <param name="textValue">Value of <see cref="string" />-type setting with unbounded length.</param>
        /// <param name="intValue">Value of <see cref="int" />-type setting.</param>
        /// <param name="boolValue">Value of <see cref="bool" />-type setting.</param>
        /// <param name="guidValue">Value of <see cref="Guid" />-type setting.</param>
        /// <param name="decimalValue">Value of <see cref="decimal" />-type setting.</param>
        /// <param name="dateTimeValue">Value of <see cref="DateTime" />-type setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <returns>Returns <c>true</c> if data successfully loaded and <c>flase</c> if loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>
        /// <para>Loading logic: user identifier AND module identifier AND setting identifier.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        public bool GetSettings(
            Guid userGuid,
            Guid moduleGuid,
            Guid settingGuid,
            out string stringValue,
            out string textValue,
            out int? intValue,
            out bool? boolValue,
            out Guid? guidValue,
            out decimal? decimalValue,
            out DateTime? dateTimeValue)
        {
            return GetSettings(
                null,
                userGuid,
                null,
                moduleGuid,
                null,
                settingGuid,
                out stringValue,
                out textValue,
                out intValue,
                out boolValue,
                out guidValue,
                out decimalValue,
                out dateTimeValue);
        }

        /// <summary>
        /// Load common setting values by name.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="stringValue">Value of <see cref="string" />-type setting with bounded length.</param>
        /// <param name="textValue">Value of <see cref="string" />-type setting with unbounded length.</param>
        /// <param name="intValue">Value of <see cref="int" />-type setting.</param>
        /// <param name="boolValue">Value of <see cref="bool" />-type setting.</param>
        /// <param name="guidValue">Value of <see cref="Guid" />-type setting.</param>
        /// <param name="decimalValue">Value of <see cref="decimal" />-type setting.</param>
        /// <param name="dateTimeValue">Value of <see cref="DateTime" />-type setting.</param>
        /// <returns>Returns <c>true</c> if data successfully loaded and <c>flase</c> if loading is disabled (<see cref="UseSettings" /> is <c>false</c>).
        /// </returns>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        public bool GetCommonSettings(
            string settingName,
            out string stringValue,
            out string textValue,
            out int? intValue,
            out bool? boolValue,
            out Guid? guidValue,
            out decimal? decimalValue,
            out DateTime? dateTimeValue)
        {
            return GetSettings(
                CommonUserName,
                CommonUserGuid,
                CommonModuleName,
                CommonModuleGuid,
                settingName,
                null,
                out stringValue,
                out textValue,
                out intValue,
                out boolValue,
                out guidValue,
                out decimalValue,
                out dateTimeValue); 
        }

        /// <summary>
        /// Load common setting values by identifier.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="stringValue">Value of <see cref="string" />-type setting with bounded length.</param>
        /// <param name="textValue">Value of <see cref="string" />-type setting with unbounded length.</param>
        /// <param name="intValue">Value of <see cref="int" />-type setting.</param>
        /// <param name="boolValue">Value of <see cref="bool" />-type setting.</param>
        /// <param name="guidValue">Value of <see cref="Guid" />-type setting.</param>
        /// <param name="decimalValue">Value of <see cref="decimal" />-type setting.</param>
        /// <param name="dateTimeValue">Value of <see cref="DateTime" />-type setting.</param>
        /// <returns>Returns <c>true</c> if data successfully loaded and <c>flase</c> if loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        public bool GetCommonSettings(
            Guid settingGuid,
            out string stringValue,
            out string textValue,
            out int? intValue,
            out bool? boolValue,
            out Guid? guidValue,
            out decimal? decimalValue,
            out DateTime? dateTimeValue)
        {
            return GetSettings(
                CommonUserName,
                CommonUserGuid,
                CommonModuleName,
                CommonModuleGuid,
                null,
                settingGuid,
                out stringValue,
                out textValue,
                out intValue,
                out boolValue,
                out guidValue,
                out decimalValue,
                out dateTimeValue);
        }

        /// <summary>
        /// Get all settings by user name or identifier.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <returns>List of user settings.</returns>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>Loading logic: user name OR identifier.</remarks>
        public List<UserSetting> GetAllSettingsByUser(string userName, Guid? userGuid)
        {
            if (!UseSettings)
                return null;

            if (userGuid == null && string.IsNullOrEmpty(userName))
                throw new ArgumentException();

            try
            {
                DataObject[] dataObjects = GetObectsWithPropsInVeiw(
                    userName, 
                    userGuid, 
                    null, 
                    null, 
                    null, 
                    null, 
                    new[] { "StrVal", "TxtVal", "IntVal", "BoolVal", "GuidVal", "DecimalVal", "DateTimeVal" });

                return dataObjects.Cast<UserSetting>().ToList();
            }
            catch (Exception ex)
            {
                LogService.LogError("Exception in loading all user settings.", ex);
                throw;
            }
        }

        /// <summary>
        /// Get all settings by user name.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>List of user settings.</returns>
        public List<UserSetting> GetAllSettingsByUser(string userName)
        {
            return GetAllSettingsByUser(userName, null);
        }

        /// <summary>
        /// Get all settings by user identifier.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <returns>List of user settings.</returns>
        public List<UserSetting> GetAllSettingsByUser(Guid userGuid)
        {
            return GetAllSettingsByUser(null, userGuid);
        }

        /// <summary>
        /// Get all names of settings by module name and identifier.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <returns>List of all names of settings.</returns>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>Loading logic: user name AND module name.</remarks>
        public List<string> GetAllSettingsNames(string userName, string moduleName)
        {
            if (!UseSettings)
                return null;

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(moduleName))
                throw new ArgumentException();

            try
            {
                string[] props = { "SettName" };
                DataObject[] dObjs = GetObectsWithPropsInVeiw(userName, null, moduleName, null, null, null, props, props);
                return dObjs.Cast<UserSetting>().Select(us => us.SettName).ToList();
            }
            catch (Exception ex)
            {
                LogService.LogError("GetAllSettingsNames", ex);
                throw;
            }
        }

        /// <summary>
        /// Get all common settings.
        /// </summary>
        /// <returns>List of all common settings.</returns>
        public List<UserSetting> GetAllCommonSettings()
        {
            return GetAllSettingsByUser(CommonUserName, CommonUserGuid);
        }

        /// <summary>
        /// Get all settings by module name or identifier.
        /// </summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <returns>List of module settings.</returns>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>Loading logic: module name OR identifier.</remarks>
        public List<UserSetting> GetAllSettingsByModule(string moduleName, Guid? moduleGuid)
        {
            if (!UseSettings)
                return null;

            if (moduleGuid == null && string.IsNullOrEmpty(moduleName))
                throw new ArgumentException();

            try
            {
                DataObject[] dObjs = GetObectsWithPropsInVeiw(
                    null, 
                    null,
                    moduleName,
                    moduleGuid, 
                    null,
                    null,
                    new[] { "StrVal", "TxtVal", "IntVal", "BoolVal", "GuidVal", "DecimalVal", "DateTimeVal" });

                return dObjs.Cast<UserSetting>().ToList();
            }
            catch (Exception ex)
            {
                LogService.LogError("GetAllSettingsByModule", ex);
                throw;
            }
        }

        /// <summary>
        /// Get all settings by module name.
        /// </summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <returns>List of module settings.</returns>
        public List<UserSetting> GetAllSettingsByModule(string moduleName)
        {
            return GetAllSettingsByModule(moduleName, null);
        }

        /// <summary>
        /// Get all settings by module identifier.
        /// </summary>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <returns>List of module settings.</returns>
        public List<UserSetting> GetAllSettingsByModule(Guid moduleGuid)
        {
            return GetAllSettingsByModule(null, moduleGuid);
        }

        /// <summary>
        /// Get value of <see cref="string" />-type setting by names and identifiers of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>
        /// <para>Loading logic: (user name OR identifier) AND (module name OR identifier) AND (setting name OR identifier).</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        public string GetStrSetting(string userName, Guid? userGuid, string moduleName, Guid? moduleGuid, string settingName, Guid? settingGuid)
        {
            return GetSettings(userName, userGuid, moduleName, moduleGuid, settingName, settingGuid, "StrVal", p => p.StrVal);
        }

        /// <summary>
        /// Get value of <see cref="string" />-type setting by names of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>
        /// <para>Loading logic: user name AND module name AND setting name.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        public string GetStrSetting(string userName, string moduleName, string settingName)
        {
            return GetStrSetting(userName, null, moduleName, null, settingName, null);
        }

        /// <summary>
        /// Get value of <see cref="string" />-type setting.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>
        /// <para>Loading logic: user identifier AND identifier name AND setting identifier.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        public string GetStrSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid)
        {
            return GetStrSetting(null, userGuid, null, moduleGuid, null, settingGuid);
        }

        /// <summary>
        /// Get value of common <see cref="string" />-type setting by name.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        public string GetCommonStrSetting(string settingName)
        {
            return GetStrSetting(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, settingName, null);
        }

        /// <summary>
        /// Get value of common <see cref="string" />-type setting by identifier.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        public string GetCommonStrSetting(Guid settingGuid)
        {
            return GetStrSetting(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, null, settingGuid);
        }

        /// <summary>
        /// Get value of <see cref="int" />-type setting by names and identifiers of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>
        /// <para>Loading logic: (user name OR identifier) AND (module name OR identifier) AND (setting name OR identifier).</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        public string GetTxtSetting(string userName, Guid? userGuid, string moduleName, Guid? moduleGuid, string settingName, Guid? settingGuid)
        {
            return GetSettings(userName, userGuid, moduleName, moduleGuid, settingName, settingGuid, "TxtVal", p => p.TxtVal);
        }

        /// <summary>
        /// Get value of <see cref="int" />-type setting by names of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>
        /// <para>Loading logic: user name AND module name AND setting name.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        public string GetTxtSetting(string userName, string moduleName, string settingName)
        {
            return GetTxtSetting(userName, null, moduleName, null, settingName, null);
        }

        /// <summary>
        /// Get value of <see cref="int" />-type setting.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>
        /// In case of multiple results only first record will be loaded. Others will be deleted.
        /// </remarks>
        public string GetTxtSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid)
        {
            return GetTxtSetting(null, userGuid, null, moduleGuid, null, settingGuid);
        }

        /// <summary>
        /// Get value of common <see cref="int" />-type setting by name.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>
        /// <para>Loading logic: user identifier AND identifier name AND setting identifier.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        public string GetCommonTxtSetting(string settingName)
        {
            return GetTxtSetting(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, settingName, null);
        }

        /// <summary>
        /// Get value of common <see cref="int" />-type setting by identifier.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        public string GetCommonTxtSetting(Guid settingGuid)
        {
            return GetTxtSetting(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, null, settingGuid);
        }

        /// <summary>
        /// Get value of <see cref="int" />-type setting by names and identifiers of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>
        /// <para>Loading logic: (user name OR identifier) AND (module name OR identifier) AND (setting name OR identifier).</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        public int? GetIntSetting(string userName, Guid? userGuid, string moduleName, Guid? moduleGuid, string settingName, Guid? settingGuid)
        {
            return GetSettings(userName, userGuid, moduleName, moduleGuid, settingName, settingGuid, "IntVal", p => p.IntVal);
        }

        /// <summary>
        /// Get value of <see cref="int" />-type setting by names of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>
        /// <para>Loading logic: user name AND module name AND setting name.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        public int? GetIntSetting(string userName, string moduleName, string settingName)
        {
            return GetIntSetting(userName, null, moduleName, null, settingName, null);
        }

        /// <summary>
        /// Get value of <see cref="int" />-type setting by identifiers of user and module.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>
        /// <para>Loading logic: user identifier AND identifier name AND setting identifier.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        public int? GetIntSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid)
        {
            return GetIntSetting(null, userGuid, null, moduleGuid, null, settingGuid);
        }

        /// <summary>
        /// Get value of common <see cref="int" />-type setting by name.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        public int? GetCommonIntSetting(string settingName)
        {
            return GetIntSetting(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, settingName, null);
        }

        /// <summary>
        /// Get value of common <see cref="int" />-type setting by identifier.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        public int? GetCommonIntSetting(Guid settingGuid)
        {
            return GetIntSetting(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, null, settingGuid);
        }

        /// <summary>
        /// Get value of <see cref="bool" />-type setting by names and identifiers of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>
        /// <para>Loading logic: (user name OR identifier) AND (module name OR identifier) AND (setting name OR identifier).</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        public bool? GetBoolSetting(string userName, Guid? userGuid, string moduleName, Guid? moduleGuid, string settingName, Guid? settingGuid)
        {
            return GetSettings(userName, userGuid, moduleName, moduleGuid, settingName, settingGuid, "BoolVal", p => p.BoolVal);
        }

        /// <summary>
        /// Get value of <see cref="bool" />-type setting by names of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>
        /// <para>Loading logic: user name AND module name AND setting name.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        public bool? GetBoolSetting(string userName, string moduleName, string settingName)
        {
            return GetBoolSetting(userName, null, moduleName, null, settingName, null);
        }

        /// <summary>
        /// Get value of <see cref="bool" />-type setting by identifiers of user and module.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>
        /// <para>Loading logic: user identifier AND identifier name AND setting identifier.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        public bool? GetBoolSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid)
        {
            return GetBoolSetting(null, userGuid, null, moduleGuid, null, settingGuid);
        }

        /// <summary>
        /// Get value of common <see cref="bool" />-type setting by name.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        public bool? GetCommonBoolSetting(string settingName)
        {
            return GetBoolSetting(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, settingName, null);
        }

        /// <summary>
        /// Get value of common <see cref="bool" />-type setting by identifier.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        public bool? GetCommonBoolSetting(Guid settingGuid)
        {
            return GetBoolSetting(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, null, settingGuid);
        }

        /// <summary>
        /// Get value of <see cref="string" />-type setting by names and identifiers of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>
        /// <para>Loading logic: (user name OR identifier) AND (module name OR identifier) AND (setting name OR identifier).</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        public Guid? GetGuidSetting(string userName, Guid? userGuid, string moduleName, Guid? moduleGuid, string settingName, Guid? settingGuid)
        {
            return GetSettings(userName, userGuid, moduleName, moduleGuid, settingName, settingGuid, "GuidVal", p => p.GuidVal);
        }

        /// <summary>
        /// Get value of <see cref="string" />-type setting by names of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>
        /// <para>Loading logic: user name AND module name AND setting name.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        public Guid? GetGuidSetting(string userName, string moduleName, string settingName)
        {
            return GetGuidSetting(userName, null, moduleName, null, settingName, null);
        }

        /// <summary>
        /// Get value of <see cref="string" />-type setting by identifiers of user and module.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>
        /// <para>Loading logic: user identifier AND identifier name AND setting identifier.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        public Guid? GetGuidSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid)
        {
            return GetGuidSetting(null, userGuid, null, moduleGuid, null, settingGuid);
        }

        /// <summary>
        /// Get value of common <see cref="string" />-type setting by name.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        public Guid? GetCommonGuidSetting(string settingName)
        {
            return GetGuidSetting(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, settingName, null);
        }

        /// <summary>
        /// Get value of <see cref="Decimal" />-type setting by names of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>
        /// <para>Loading logic: user name AND module name AND setting name.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        public Decimal? GetDecimalSetting(string userName, string moduleName, string settingName)
        {
            return GetDecimalSetting(userName, null, moduleName, null, settingName, null);
        }

        /// <summary>
        /// Get value of common <see cref="string" />-type setting by identifier.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        public Guid? GetCommonGuidSetting(Guid settingGuid)
        {
            return GetGuidSetting(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, null, settingGuid);
        }

        /// <summary>
        /// Get value of <see cref="Decimal" />-type setting by names and identifiers of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>
        /// <para>Loading logic: (user name OR identifier) AND (module name OR identifier) AND (setting name OR identifier).</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        public Decimal? GetDecimalSetting(string userName, Guid? userGuid, string moduleName, Guid? moduleGuid, string settingName, Guid? settingGuid)
        {
            return GetSettings(userName, userGuid, moduleName, moduleGuid, settingName, settingGuid, "DecimalVal", p => p.DecimalVal);
        }

        /// <summary>
        /// Get value of <see cref="Decimal" />-type setting by identifiers of user and module.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>
        /// <para>Loading logic: user identifier AND identifier name AND setting identifier.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        public Decimal? GetDecimalSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid)
        {
            return GetDecimalSetting(null, userGuid, null, moduleGuid, null, settingGuid);
        }

        /// <summary>
        /// Get value of common <see cref="Decimal" />-type setting by name.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        public Decimal? GetCommonDecimalSetting(string settingName)
        {
            return GetDecimalSetting(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, settingName, null);
        }

        /// <summary>
        /// Get value of common <see cref="Decimal" />-type setting by identifier.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        public Decimal? GetCommonDecimalSetting(Guid settingGuid)
        {
            return GetDecimalSetting(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, null, settingGuid);
        }

        /// <summary>
        /// Get value of <see cref="DateTime" />-type setting by names and identifiers of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>
        /// <para>Loading logic: (user name OR identifier) AND (module name OR identifier) AND (setting name OR identifier).</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        public DateTime? GetDateTimeSetting(string userName, Guid? userGuid, string moduleName, Guid? moduleGuid, string settingName, Guid? settingGuid)
        {
            return GetSettings(userName, userGuid, moduleName, moduleGuid, settingName, settingGuid, "DateTimeVal", p => p.DateTimeVal);
        }

        /// <summary>
        /// Get value of <see cref="DateTime" />-type setting by names of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>
        /// <para>Loading logic: user name AND module name AND setting name.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        public DateTime? GetDateTimeSetting(string userName, string moduleName, string settingName)
        {
            return GetDateTimeSetting(userName, null, moduleName, null, settingName, null);
        }

        /// <summary>
        /// Get value of <see cref="DateTime" />-type setting by identifiers of user and module.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>
        /// <para>Loading logic: user identifier AND identifier name AND setting identifier.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        public DateTime? GetDateTimeSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid)
        {
            return GetDateTimeSetting(null, userGuid, null, moduleGuid, null, settingGuid);
        }

        /// <summary>
        /// Get value of common <see cref="DateTime" />-type setting by name.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        public DateTime? GetCommonDateTimeSetting(string settingName)
        {
            return GetDateTimeSetting(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, settingName, null);
        }

        /// <summary>
        /// Get value of common <see cref="DateTime" />-type setting by identifier.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        public DateTime? GetCommonDateTimeSetting(Guid settingGuid)
        {
            return GetDateTimeSetting(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, null, settingGuid);
        }

        #endregion

        #region Saving

        /// <summary>
        /// Set setting value using identifiers and names.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="stringValue">Value of <see cref="string" />-type setting with bounded length.</param>
        /// <param name="textValue">Value of <see cref="string" />-type setting with unbounded length.</param>
        /// <param name="intValue">Value of <see cref="int" />-type setting.</param>
        /// <param name="boolValue">Value of <see cref="bool" />-type setting.</param>
        /// <param name="guidValue">Value of <see cref="Guid" />-type setting.</param>
        /// <param name="decimalValue">Value of <see cref="decimal" />-type setting.</param>
        /// <param name="dateTimeValue">Value of <see cref="DateTime" />-type setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        public bool SetSettings(
            string userName,
            Guid? userGuid,
            string moduleName,
            Guid? moduleGuid,
            string settingName,
            Guid? settingGuid,
            string stringValue,
            string textValue,
            int? intValue,
            bool? boolValue,
            Guid? guidValue,
            decimal? decimalValue,
            DateTime? dateTimeValue)
        {
            if (!UseSettings)
                return false;

            if (CheckSettingIdentityIsNull(userName, userGuid, moduleName, moduleGuid, settingName, settingGuid))
                throw new ArgumentException();
            
            if (stringValue != null && stringValue.Length > 256)
                throw new ArgumentOutOfRangeException("stringValue", "stringValue не может содержать больше 256 символов. Пришла строка [" + stringValue + "] длиной " + stringValue.Length);

            bool retBool;
            try
            {
                IDataService ds = DataService;

                var view = new View { DefineClassType = typeof(UserSetting) };
                view.AddProperty("UserName");
                view.AddProperty("UserGuid");
                view.AddProperty("ModuleName");
                view.AddProperty("ModuleGuid");
                view.AddProperty("SettName");
                view.AddProperty("SettGuid");
                view.AddProperty("StrVal");
                view.AddProperty("TxtVal");
                view.AddProperty("IntVal");
                view.AddProperty("BoolVal");
                view.AddProperty("GuidVal");
                view.AddProperty("DecimalVal");
                view.AddProperty("DateTimeVal");
                // v.AddProperty("SettLastAccessTime");

                LoadingCustomizationStruct lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(UserSetting), view);
                lcs.InitDataCopy = false;   // Если будем обновлять, то сделаем InitDtaCopy сами.
                lcs.LimitFunction = GetLimitFunctionBySettIDs(userName, userGuid, moduleName, moduleGuid, settingName, settingGuid);
                
                DataObject[] dObjs = ds.LoadObjects(lcs);
                
                // если все свойства пустые, то удалим настройку из БД (надо написать метод для удаления настроек, который будет просто вызывать этот метод)
                if (stringValue == null && textValue == null && intValue == null && boolValue == null && guidValue == null && decimalValue == null && dateTimeValue == null)
                {
                    DeleteObjects(ds, dObjs);

                    retBool = true;
                }
                else
                {
                    // первый из найденых обновляем, остальные удаляем - избавляемся от дублей
                    UserSetting userSett;
                    if (dObjs.Length > 0)
                    {
                        userSett = (UserSetting)dObjs[0];
                        userSett.EnableInitDataCopy();
                        userSett.InitDataCopy();
                        userSett.AddLoadedProperties("SettLastAccessTime");
                        userSett.SettLastAccessTime = DateTime.Now;

                        DeleteObjectsFrom1ToN(ds, dObjs);
                    }
                    else
                    {
                        userSett = new UserSetting();
                    }

                    userSett.UserName = userName;
                    userSett.UserGuid = userGuid;
                    userSett.ModuleName = moduleName;
                    userSett.ModuleGuid = moduleGuid;
                    userSett.SettName = settingName;
                    userSett.SettGuid = settingGuid;

                    userSett.StrVal = stringValue;
                    userSett.TxtVal = textValue;
                    userSett.IntVal = intValue;
                    userSett.BoolVal = boolValue;
                    userSett.GuidVal = guidValue;
                    userSett.DecimalVal = decimalValue;
                    userSett.DateTimeVal = dateTimeValue;
                    userSett.DisableInitDataCopy(); // Обязательно надо делать, потому что эти объекты больше никуда не идут.
                    
                    DataObject dObj = userSett;
                    ds.UpdateObject(ref dObj);

                    retBool = true;
                }
            }
            catch (Exception ex)
            {
                LogService.LogError(string.Format("SetSettings({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12})", userName, userGuid, moduleName, moduleGuid, settingName, settingGuid, stringValue, textValue, intValue, boolValue, guidValue, decimalValue, dateTimeValue), ex);
                throw;
            }

            return retBool;
        }

        /// <summary>
        /// Set setting value using names.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="stringValue">Value of <see cref="string" />-type setting with bounded length.</param>
        /// <param name="textValue">Value of <see cref="string" />-type setting with unbounded length.</param>
        /// <param name="intValue">Value of <see cref="int" />-type setting.</param>
        /// <param name="boolValue">Value of <see cref="bool" />-type setting.</param>
        /// <param name="guidValue">Value of <see cref="Guid" />-type setting.</param>
        /// <param name="decimalValue">Value of <see cref="decimal" />-type setting.</param>
        /// <param name="dateTimeValue">Value of <see cref="DateTime" />-type setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetSettings(string userName, string moduleName, string settingName, string stringValue, string textValue, int? intValue, bool? boolValue, Guid? guidValue, decimal? decimalValue, DateTime? dateTimeValue)
        {
            return SetSettings(userName, null, moduleName, null, settingName, null, stringValue, textValue, intValue, boolValue, guidValue, decimalValue, dateTimeValue);
        }

        /// <summary>
        /// Set <see cref="string" />-type setting.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="stringValue">Value of <see cref="string" />-type setting with bounded length.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetStrSetting(string userName, string moduleName, string settingName, string stringValue)
        {
            return SetSettings(userName, null, moduleName, null, settingName, null, stringValue, null, null, null, null, null, null);
        }

        /// <summary>
        /// Set <see cref="string" />-type setting.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="stringValue">Value of <see cref="string" />-type setting with bounded length.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetStrSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid, string stringValue)
        {
            return SetSettings(null, userGuid, null, moduleGuid, null, settingGuid, stringValue, null, null, null, null, null, null);
        }

        /// <summary>
        /// Set common <see cref="string" />-type setting.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="stringValue">Value of <see cref="string" />-type setting with bounded length.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetCommonStrSetting(string settingName, string stringValue)
        {
            return SetSettings(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, settingName, null, stringValue, null, null, null, null, null, null);
        }

        /// <summary>
        /// Set common <see cref="string" />-type setting.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="stringValue">Value of <see cref="string" />-type setting with bounded length.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetCommonStrSetting(Guid settingGuid, string stringValue)
        {
            return SetSettings(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, null, settingGuid, stringValue, null, null, null, null, null, null);
        }

        /// <summary>
        /// Set <see cref="string" />-type setting with unbounded length.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="textValue">Value of <see cref="string" />-type setting with unbounded length.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetTxtSetting(string userName, string moduleName, string settingName, string textValue)
        {
            return SetSettings(userName, null, moduleName, null, settingName, null, null, textValue, null, null, null, null, null);
        }

        /// <summary>
        /// Set <see cref="string" />-type setting with unbounded length.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="textValue">Value of <see cref="string" />-type setting with unbounded length.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetTxtSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid, string textValue)
        {
            return SetSettings(null, userGuid, null, moduleGuid, null, settingGuid, null, textValue, null, null, null, null, null);
        }

        /// <summary>
        /// Set common <see cref="string" />-type setting with unbounded length.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="textValue">Value of <see cref="string" />-type setting with unbounded length.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetCommonTxtSetting(string settingName, string textValue)
        {
            return SetSettings(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, settingName, null, null, textValue, null, null, null, null, null);
        }

        /// <summary>
        /// Set common <see cref="string" />-type setting with unbounded length.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="textValue">Value of <see cref="string" />-type setting with unbounded length.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetCommonTxtSetting(Guid settingGuid, string textValue)
        {
            return SetSettings(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, null, settingGuid, null, textValue, null, null, null, null, null);
        }

        /// <summary>
        /// Set <see cref="int" />-type setting.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="intValue">Value of <see cref="int" />-type setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetIntSetting(string userName, string moduleName, string settingName, int intValue)
        {
            return SetSettings(userName, null, moduleName, null, settingName, null, null, null, intValue, null, null, null, null);
        }

        /// <summary>
        /// Set <see cref="int" />-type setting.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="intValue">Value of <see cref="int" />-type setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetIntSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid, int intValue)
        {
            return SetSettings(null, userGuid, null, moduleGuid, null, settingGuid, null, null, intValue, null, null, null, null);
        }

        /// <summary>
        /// Set common <see cref="int" />-type setting.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="intValue">Value of <see cref="int" />-type setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetCommonIntSetting(string settingName, int intValue)
        {
            return SetSettings(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, settingName, null, null, null, intValue, null, null, null, null);
        }

        /// <summary>
        /// Set common <see cref="int" />-type setting.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="intValue">Value of <see cref="int" />-type setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetCommonIntSetting(Guid settingGuid, int intValue)
        {
            return SetSettings(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, null, settingGuid, null, null, intValue, null, null, null, null);
        }

        /// <summary>
        /// Set <see cref="bool" />-type setting.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="boolValue">Value of <see cref="bool" />-type setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetBoolSetting(string userName, string moduleName, string settingName, bool boolValue)
        {
            return SetSettings(userName, null, moduleName, null, settingName, null, null, null, null, boolValue, null, null, null);
        }

        /// <summary>
        /// Set <see cref="bool" />-type setting.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="boolValue">Value of <see cref="bool" />-type setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetBoolSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid, bool boolValue)
        {
            return SetSettings(null, userGuid, null, moduleGuid, null, settingGuid, null, null, null, boolValue, null, null, null);
        }

        /// <summary>
        /// Set common <see cref="bool" />-type setting.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="boolValue">Value of <see cref="bool" />-type setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetCommonBoolSetting(string settingName, bool boolValue)
        {
            return SetSettings(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, settingName, null, null, null, null, boolValue, null, null, null);
        }

        /// <summary>
        /// Set common <see cref="bool" />-type setting.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="boolValue">Value of <see cref="bool" />-type setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetCommonBoolSetting(Guid settingGuid, bool boolValue)
        {
            return SetSettings(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, null, settingGuid, null, null, null, boolValue, null, null, null);
        }

        /// <summary>
        /// Set <see cref="string" />-type setting.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="guidValue">Value of <see cref="Guid" />-type setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetGuidSetting(string userName, string moduleName, string settingName, Guid guidValue)
        {
            return SetSettings(userName, null, moduleName, null, settingName, null, null, null, null, null, guidValue, null, null);
        }

        /// <summary>
        /// Set <see cref="string" />-type setting.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="guidValue">Value of <see cref="Guid" />-type setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetGuidSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid, Guid guidValue)
        {
            return SetSettings(null, userGuid, null, moduleGuid, null, settingGuid, null, null, null, null, guidValue, null, null);
        }

        /// <summary>
        /// Set common <see cref="string" />-type setting.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="guidValue">Value of <see cref="Guid" />-type setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetCommonGuidSetting(string settingName, Guid guidValue)
        {
            return SetSettings(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, settingName, null, null, null, null, null, guidValue, null, null);
        }

        /// <summary>
        /// Set common <see cref="string" />-type setting.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="guidValue">Value of <see cref="Guid" />-type setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetCommonGuidSetting(Guid settingGuid, Guid guidValue)
        {
            return SetSettings(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, null, settingGuid, null, null, null, null, guidValue, null, null);
        }

        /// <summary>
        /// Set <see cref="Decimal" />-type setting.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="decimalValue">Value of <see cref="decimal" />-type setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetDecimalSetting(string userName, string moduleName, string settingName, Decimal decimalValue)
        {
            return SetSettings(userName, null, moduleName, null, settingName, null, null, null, null, null, null, decimalValue, null);
        }

        /// <summary>
        /// Set <see cref="Decimal" />-type setting.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="decimalValue">Value of <see cref="decimal" />-type setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetDecimalSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid, Decimal decimalValue)
        {
            return SetSettings(null, userGuid, null, moduleGuid, null, settingGuid, null, null, null, null, null, decimalValue, null);
        }

        /// <summary>
        /// Set common <see cref="Decimal" />-type setting.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="decimalValue">Value of <see cref="decimal" />-type setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetCommonDecimalSetting(string settingName, Decimal decimalValue)
        {
            return SetSettings(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, settingName, null, null, null, null, null, null, decimalValue, null);
        }

        /// <summary>
        /// Set common <see cref="Decimal" />-type setting.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="decimalValue">Value of <see cref="decimal" />-type setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetCommonDecimalSetting(Guid settingGuid, Decimal decimalValue)
        {
            return SetSettings(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, null, settingGuid, null, null, null, null, null, decimalValue, null);
        }

        /// <summary>
        /// Set <see cref="DateTime" />-type setting.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="dateTimeValue">Value of <see cref="DateTime" />-type setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetDateTimeSetting(string userName, string moduleName, string settingName, DateTime dateTimeValue)
        {
            return SetSettings(userName, null, moduleName, null, settingName, null, null, null, null, null, null, null, dateTimeValue);
        }

        /// <summary>
        /// Set <see cref="DateTime" />-type setting.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="dateTimeValue">Value of <see cref="DateTime" />-type setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetDateTimeSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid, DateTime dateTimeValue)
        {
            return SetSettings(null, userGuid, null, moduleGuid, null, settingGuid, null, null, null, null, null, null, dateTimeValue);
        }

        /// <summary>
        /// Set common <see cref="DateTime" />-type setting.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="dateTimeValue">Value of <see cref="DateTime" />-type setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetCommonDateTimeSetting(string settingName, DateTime dateTimeValue)
        {
            return SetSettings(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, settingName, null, null, null, null, null, null, null, dateTimeValue);
        }

        /// <summary>
        /// Set common <see cref="DateTime" />-type setting.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="dateTimeValue">Value of <see cref="DateTime" />-type setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetCommonDateTimeSetting(Guid settingGuid, DateTime dateTimeValue)
        {
            return SetSettings(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, null, settingGuid, null, null, null, null, null, null, dateTimeValue);
        }

        /// <summary>
        /// Set setting value using identifiers.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="stringValue">Value of <see cref="string" />-type setting with bounded length.</param>
        /// <param name="textValue">Value of <see cref="string" />-type setting with unbounded length.</param>
        /// <param name="intValue">Value of <see cref="int" />-type setting.</param>
        /// <param name="boolValue">Value of <see cref="bool" />-type setting.</param>
        /// <param name="guidValue">Value of <see cref="Guid" />-type setting.</param>
        /// <param name="decimalValue">Value of <see cref="decimal" />-type setting.</param>
        /// <param name="dateTimeValue">Value of <see cref="DateTime" />-type setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool SetSettings(Guid userGuid, Guid moduleGuid, Guid settingGuid, string stringValue, string textValue, int? intValue, bool? boolValue, Guid? guidValue, decimal? decimalValue, DateTime? dateTimeValue)
        {
            return SetSettings(null, userGuid, null, moduleGuid, null, settingGuid, stringValue, textValue, intValue, boolValue, guidValue, decimalValue, dateTimeValue);
        }
        
        #endregion

        #region Deleting

        /// <summary>
        /// Delete all settings by user name.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool DeleteSettingsByUser(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                return false;

            return DeleteSettings(userName, null, null, null, null, null);
        }

        /// <summary>
        /// Delete all settings by user identifier.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool DeleteSettingsByUser(Guid userGuid)
        {
            return DeleteSettings(null, userGuid, null, null, null, null);
        }

        /// <summary>
        /// Delete all settings by user name and identifier.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        /// <remarks>Deletes data using user name OR identifier.</remarks>
        public bool DeleteSettingsByUser(string userName, Guid? userGuid)
        {
            if (string.IsNullOrEmpty(userName) && userGuid == null)
                return false;

            return DeleteSettings(userName, userGuid, null, null, null, null);
        }

        /// <summary>
        /// Delete all settings by module name or identifier.
        /// </summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        /// <remarks>Deletes data using module name OR identifier.</remarks>
        public bool DeleteSettingsByModule(string moduleName, Guid? moduleGuid)
        {
            if (string.IsNullOrEmpty(moduleName) && moduleGuid == null)
                return false;

            return DeleteSettings(null, null, moduleName, moduleGuid, null, null);
        }

        /// <summary>
        /// Delete all settings by module and setting name or identifier.
        /// </summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool DeleteSettings(string moduleName, Guid? moduleGuid, string settingName, Guid? settingGuid)
        {
            if (string.IsNullOrEmpty(settingName) && settingName == null)
                return false;

            return DeleteSettings(null, null, moduleName, moduleGuid, settingName, settingGuid);
        }

        /// <summary>
        /// Delete all settings.
        /// </summary>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool DeleteSettings()
        {
            return DeleteSettings(null, null, null, null, null, null);
        }

        /// <summary>
        /// Delete setting by names.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool DeleteSettings(string userName, string moduleName, string settingName)
        {
            return DeleteSettings(userName, null, moduleName, null, settingName, null);
        }

        /// <summary>
        /// Delete setting by identifiers.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool DeleteSettings(Guid userGuid, Guid moduleGuid, Guid settingGuid)
        {
            return DeleteSettings(null, userGuid, null, moduleGuid, null, settingGuid);
        }

        /// <summary>
        /// Delete common setting by name.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool DeleteCommonSettings(string settingName)
        {
            return DeleteSettings(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, settingName, null);
        }

        /// <summary>
        /// Delete all common settings.
        /// </summary>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool DeleteAllCommonSettings()
        {
            return DeleteSettings(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, null, null);
        }

        /// <summary>
        /// Delete common setting by identifier.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        public bool DeleteCommonSettings(Guid settingGuid)
        {
            return DeleteSettings(CommonUserName, CommonUserGuid, CommonModuleName, CommonModuleGuid, null, settingGuid);
        }

        /// <summary>
        /// Delete settings.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        /// <remarks>Deletes all settings usin OR logic for non-null arguments.</remarks>
        public bool DeleteSettings(string userName, Guid? userGuid, string moduleName, Guid? moduleGuid, string settingName, Guid? settingGuid)
        {
            if (!UseSettings)
                return false;

            // Вычитаем всё, что нам подходит по параметрам.
            bool retBool = false;
            View v = null;
            Function func = GetLimitFunctionAndViewBySomeIDs(userName, userGuid, moduleName, moduleGuid, settingName, settingGuid, out v);

            try
            {
                IDataService ds = DataService;
                LoadingCustomizationStruct lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(UserSetting), v);
                lcs.InitDataCopy = false; // Обновлять не надо - будет удаление.
                lcs.LimitFunction = func;
                lcs.ReturnTop = 500;
                bool empty = false;
                do
                {
                    DataObject[] dObjs = ds.LoadObjects(lcs);
                    empty = dObjs.Length == 0;
                    DeleteObjects(ds, dObjs);
                }
                while (!empty);

                retBool = true;
            }
            catch (Exception ex)
            {
                LogService.LogError("DeleteSettings", ex);
            }

            return retBool;
        }

        #endregion

        /// <summary>
        /// Gets the limit function for loading setting by name or by identifer of some field.
        /// Adds properties in specified view.
        /// Uses logic defined in <see cref="IUserSettingsService"/>: load by name OR identifier.
        /// </summary>
        /// <param name="nameValue">Value of the name property.</param>
        /// <param name="nameProperty">Name of the name property.</param>
        /// <param name="guidValue">Value of the identifier property.</param>
        /// <param name="guidProperty">Name of the identifier property.</param>
        /// <param name="view">The view where is need to add properties for loading data.</param>
        /// <returns>Limit function for loading setting.</returns>
        private static Function BuildNameOrGuidFunction(string nameValue, string nameProperty, Guid? guidValue, string guidProperty, View view = null)
        {
            SQLWhereLanguageDef langdef = SQLWhereLanguageDef.LanguageDef;

            Function funcName = null;
            if (!string.IsNullOrEmpty(nameValue))
            {
                funcName = langdef.GetFunction(langdef.funcEQ, new VariableDef(langdef.StringType, nameProperty), nameValue);

                if (view != null)
                    view.AddProperty(nameProperty);
            }

            Function funcGuid = null;
            if (guidValue != null)
            {
                funcGuid = langdef.GetFunction(langdef.funcEQ, new VariableDef(langdef.GuidType, guidProperty), guidValue.Value);

                if (view != null)
                    view.AddProperty(guidProperty);
            }

            Function result;
            if (funcName != null)
            {
                result = funcGuid != null
                    ? langdef.GetFunction(langdef.funcOR, funcName, funcGuid)
                    : funcName;
            }
            else
            {
                result = funcGuid;
            }

            return result;
        }

        /// <summary>
        /// Gets the limit function for loading settings.
        /// Loading logic: (userName OR userGuid) AND (moduleName OR moduleGuid) AND (settingName OR settingGuid).
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>LimitFunction function for loading settings.</returns>
        private static Function GetLimitFunctionBySettIDs(string userName, Guid? userGuid, string moduleName, Guid? moduleGuid, string settingName, Guid? settingGuid)
        {
            Function funcUser = BuildNameOrGuidFunction(userName, "UserName", userGuid, "UserGuid");
            Function funcModule = BuildNameOrGuidFunction(moduleName, "ModuleName", moduleGuid, "ModuleGuid");
            Function funcSett = BuildNameOrGuidFunction(settingName, "SettName", settingGuid, "SettGuid");

            SQLWhereLanguageDef langdef = SQLWhereLanguageDef.LanguageDef;
            return langdef.GetFunction(langdef.funcAND, funcUser, funcModule, funcSett);
        }

        /// <summary>
        /// Gets the limit function loading settings and builds appropriate view.
        /// Loading logic: (userName OR userGuid) AND (moduleName OR moduleGuid) AND (settingName OR settingGuid).
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="view">The view.</param>
        /// <returns>LimitFunction function for loading settings.</returns>
        private static Function GetLimitFunctionAndViewBySomeIDs(string userName, Guid? userGuid, string moduleName, Guid? moduleGuid, string settingName, Guid? settingGuid, out View view)
        {
            view = new View { DefineClassType = typeof(UserSetting) };

            Function funcUser = BuildNameOrGuidFunction(userName, "UserName", userGuid, "UserGuid", view);
            Function funcModule = BuildNameOrGuidFunction(moduleName, "ModuleName", moduleGuid, "ModuleGuid", view);
            Function funcSett = BuildNameOrGuidFunction(settingName, "SettName", settingGuid, "SettGuid", view);

            // Тут надо вернуть функцию с учётом того, что не все функции есть.
            List<Function> funcs = new List<Function>();
            if (funcUser != null)
                funcs.Add(funcUser);

            if (funcModule != null)
                funcs.Add(funcModule);

            if (funcSett != null)
                funcs.Add(funcSett);

            Function lf = null;
            SQLWhereLanguageDef langdef = SQLWhereLanguageDef.LanguageDef;

            if (funcs.Count > 1)
                lf = langdef.GetFunction(langdef.funcAND, funcs.ToArray());
            else if (funcs.Count == 1)
                lf = funcs[0];

            return lf;
        }

        /// <summary>
        /// Checks parameters for loading or saving setting.
        /// We need a possibility to correctly load (or save) data.
        ///  </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Returns <c>true</c> if we can correctly load or save settings by specified parameters.</returns>
        private static bool CheckSettingIdentityIsNull(string userName, Guid? userGuid, string moduleName, Guid? moduleGuid, string settingName, Guid? settingGuid)
        {
            return (string.IsNullOrEmpty(userName) && userGuid == null)
                || (string.IsNullOrEmpty(moduleName) && moduleGuid == null)
                || (string.IsNullOrEmpty(settingName) && settingGuid == null);
        }

        /// <summary>
        /// Loads settings objects.
        /// </summary>
        /// <param name="view">View for loading settings.</param>
        /// <param name="lf">Limit function for loading settings.</param>
        /// <param name="ds">Data service for loading settings.</param>
        /// <param name="orderByProps">Names of properties for sorting.</param>
        /// <returns>Array of loaded data objects with settings (<see cref="UserSetting"/> instances).</returns>
        private static DataObject[] GetObjects(View view, Function lf, IDataService ds, IEnumerable<string> orderByProps = null)
        {
            LoadingCustomizationStruct lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(UserSetting), view);
            lcs.InitDataCopy = false; // Если будем обновлять, то сделаем InitDtaCopy сами.
            lcs.LimitFunction = lf;

            if (orderByProps != null)
                lcs.ColumnsSort = orderByProps.Select(s => new ColumnsSortDef(s, SortOrder.Asc)).ToArray();

            return ds.LoadObjects(lcs);
        }

        /// <summary>
        /// Deletes all objects except the first.
        /// </summary>
        /// <param name="ds">Data service for deleting objects.</param>
        /// <param name="dObjs">Objects for deleting.</param>
        private static void DeleteObjectsFrom1ToN(IDataService ds, DataObject[] dObjs)
        {
            DataObject[] objectsForDeleting = dObjs.Skip(1).ToArray();
            DeleteObjects(ds, objectsForDeleting);
        }

        /// <summary>
        /// Deletes all objects.
        /// </summary>
        /// <param name="ds">Data service for deleting objects.</param>
        /// <param name="dObjs">Objects for deleting.</param>
        private static void DeleteObjects(IDataService ds, DataObject[] dObjs)
        {
            if (dObjs.Any())
            {
                Action<DataObject> actionForDeleting = i => i.SetStatus(ObjectStatus.Deleted);
                Array.ForEach(dObjs, actionForDeleting);

                ds.UpdateObjects(ref dObjs);
            }
        }

        /// <summary>
        /// Loads settings objects only with specified properties.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="propsNames">Names of properties for loading.</param>
        /// <param name="orderByProps">Names of properties for sorting.</param>
        /// <returns>Array of loaded data objects with settings (<see cref="UserSetting"/> instances).</returns>
        private DataObject[] GetObectsWithPropsInVeiw(
            string userName,
            Guid? userGuid,
            string moduleName,
            Guid? moduleGuid,
            string settingName,
            Guid? settingGuid,
            IEnumerable<string> propsNames,
            IEnumerable<string> orderByProps = null)
        {
            View view;
            Function lf = GetLimitFunctionAndViewBySomeIDs(userName, userGuid, moduleName, moduleGuid, settingName, settingGuid, out view);
            foreach (string propName in propsNames)
                view.AddProperty(propName);

            return GetObjects(view, lf, DataService, orderByProps);
        }

        /// <summary>
        /// Gets single value of specified setting from data service.
        /// </summary>
        /// <typeparam name="T">The type of setting.</typeparam>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">The user unique identifier.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">The module unique identifier.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">The setting unique identifier.</param>
        /// <param name="prop">The name property for loading.</param>
        /// <param name="loader">Delegate for loading value of setting from loaded <see cref="UserSetting"/> instance.</param>
        /// <returns>Value of setting or <c>default(T)</c> if it's not found or loading is disabled (<see cref="UseSettings" /> is <c>false</c>).</returns>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        private T GetSettings<T>(
            string userName,
            Guid? userGuid,
            string moduleName,
            Guid? moduleGuid,
            string settingName,
            Guid? settingGuid,
            string prop,
            Func<UserSetting, T> loader)
        {
            T result = default(T);

            if (!UseSettings)
                return result;

            if (CheckSettingIdentityIsNull(userName, userGuid, moduleName, moduleGuid, settingName, settingGuid))
                throw new ArgumentException();

            try
            {
                DataObject[] dObjs = GetObectsWithPropsInVeiw(userName, userGuid, moduleName, moduleGuid, settingName, settingGuid, new[] { prop });
                if (dObjs.Length > 0)
                {
                    DeleteObjectsFrom1ToN(DataService, dObjs);
                    UserSetting userSett = (UserSetting)dObjs[0];
                    result = loader(userSett);
                }

                return result;
            }
            catch (Exception ex)
            {
                LogService.LogError("GetSettings", ex);
                throw;
            }
        }
    }
}
