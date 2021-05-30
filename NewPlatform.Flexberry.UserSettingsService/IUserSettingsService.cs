namespace ICSSoft.Services
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface of service for working with settings in application.
    /// </summary>
    /// <remarks>
    /// <para>All settings are loaded only for current application (<see cref="AppName"/>).</para>
    /// <para>http://wiki.ics.perm.ru/UserSettingsService.ashx</para>
    /// </remarks>
    public interface IUserSettingsService
    {
        #region Configuration

        /// <summary>
        /// Flag of enabled service.
        /// </summary>
        bool UseSettings { get; set; }

        /// <summary>
        /// Name of application for loading data.
        /// </summary>
        string AppName { get; }

        /// <summary>
        /// Name of module that is used for common settings.
        /// </summary>
        string CommonModuleName { get; }

        /// <summary>
        /// Name of user that is used for common settings.
        /// </summary>
        string CommonUserName { get; }

        /// <summary>
        /// Identifier of module that is used for common settings.
        /// </summary>
        Guid CommonModuleGuid { get; }

        /// <summary>
        /// Identifier of user that is used for common settings.
        /// </summary>
        Guid CommonUserGuid { get; }

        #endregion

        /// <summary>
        /// Get the name of user from AD.
        /// </summary>
        /// <param name="userName">Name of user for search in AD.</param>
        /// <returns>Name of user from AD or <c>null</c>, if it's not found.</returns>
        string GetADUserName(string userName);

        /// <summary>
        /// Get the name of current user (using <see cref="Environment.UserName"/>) from AD.
        /// </summary>
        /// <returns>Name of current user from AD or <see cref="Environment.UserName"/>, if it's not found.</returns>
        string GetEnvOrADUserName();

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
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>
        /// <para>Loading logic: (user name OR identifier) AND (module name OR identifier) AND (setting name OR identifier).</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        /// <returns>Returns <c>true</c> if data successfully loaded and <c>flase</c> if loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        bool GetSettings(
            string userName,
            Guid? userGuid,
            string moduleName,
            Guid? moduleGuid,
            string settingName,
            Guid? settingGuid,
            out string stringValue,
            out string textValue,
            out int? intValue,
            out bool? boolValue,
            out Guid? guidValue,
            out decimal? decimalValue,
            out DateTime? dateTimeValue);

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
        /// <remarks>
        /// <para>Loading logic: user name AND module name AND setting name.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        /// <returns>Returns <c>true</c> if data successfully loaded and <c>flase</c> if loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        bool GetSettings(
            string userName,
            string moduleName,
            string settingName,
            out string stringValue,
            out string textValue,
            out int? intValue,
            out bool? boolValue,
            out Guid? guidValue,
            out decimal? decimalValue,
            out DateTime? dateTimeValue);

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
        /// <remarks>
        /// <para>Loading logic: user identifier AND module identifier AND setting identifier.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        /// <returns>Returns <c>true</c> if data successfully loaded and <c>flase</c> if loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        bool GetSettings(
            Guid userGuid,
            Guid moduleGuid,
            Guid settingGuid,
            out string stringValue,
            out string textValue,
            out int? intValue,
            out bool? boolValue,
            out Guid? guidValue,
            out decimal? decimalValue,
            out DateTime? dateTimeValue);

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
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        /// <returns>Returns <c>true</c> if data successfully loaded and <c>flase</c> if loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        bool GetCommonSettings(
            string settingName,
            out string stringValue,
            out string textValue,
            out int? intValue,
            out bool? boolValue,
            out Guid? guidValue,
            out decimal? decimalValue,
            out DateTime? dateTimeValue);

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
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        /// <returns>Returns <c>true</c> if data successfully loaded and <c>flase</c> if loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        bool GetCommonSettings(
            Guid settingGuid,
            out string stringValue,
            out string textValue,
            out int? intValue,
            out bool? boolValue,
            out Guid? guidValue,
            out decimal? decimalValue,
            out DateTime? dateTimeValue);

        #endregion

        #region Loading all

        /// <summary>
        /// Get all settings by user name or identifier.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>Loading logic: user name OR identifier.</remarks>
        /// <returns>List of user settings.</returns>
        List<UserSetting> GetAllSettingsByUser(string userName, Guid? userGuid);

        /// <summary>
        /// Get all settings by user name.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <returns>List of user settings.</returns>
        List<UserSetting> GetAllSettingsByUser(string userName);

        /// <summary>
        /// Get all settings by user identifier.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <returns>List of user settings.</returns>
        List<UserSetting> GetAllSettingsByUser(Guid userGuid);

        /// <summary>
        /// Get all settings by module name or identifier.
        /// </summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>Loading logic: module name OR identifier.</remarks>
        /// <returns>List of module settings.</returns>
        List<UserSetting> GetAllSettingsByModule(string moduleName, Guid? moduleGuid);

        /// <summary>
        /// Get all settings by module name.
        /// </summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <returns>List of module settings.</returns>
        List<UserSetting> GetAllSettingsByModule(string moduleName);

        /// <summary>
        /// Get all settings by module identifier.
        /// </summary>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <returns>List of module settings.</returns>
        List<UserSetting> GetAllSettingsByModule(Guid moduleGuid);

        /// <summary>
        /// Get all names of settings by module name and identifier.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>Loading logic: user name AND module name.</remarks>
        /// <returns>List of all names of settings.</returns>
        List<string> GetAllSettingsNames(string userName, string moduleName);

        /// <summary>
        /// Get all common settings.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <returns>List of all common settings.</returns>
        List<UserSetting> GetAllCommonSettings();

        #endregion

        #region Loading typed

        /// <summary>
        /// Get value of <see cref="string" />-type setting by names and identifiers of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>
        /// <para>Loading logic: (user name OR identifier) AND (module name OR identifier) AND (setting name OR identifier).</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        string GetStrSetting(string userName, Guid? userGuid, string moduleName, Guid? moduleGuid, string settingName, Guid? settingGuid);

        /// <summary>
        /// Get value of <see cref="string" />-type setting by names of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>
        /// <para>Loading logic: user name AND module name AND setting name.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        string GetStrSetting(string userName, string moduleName, string settingName);

        /// <summary>
        /// Get value of <see cref="string" />-type setting.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>
        /// <para>Loading logic: user identifier AND identifier name AND setting identifier.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        string GetStrSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid);

        /// <summary>
        /// Get value of common <see cref="string" />-type setting by name.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        string GetCommonStrSetting(string settingName);

        /// <summary>
        /// Get value of common <see cref="string" />-type setting by identifier.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        string GetCommonStrSetting(Guid settingGuid);

        /// <summary>
        /// Get value of <see cref="int" />-type setting by names and identifiers of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>
        /// <para>Loading logic: (user name OR identifier) AND (module name OR identifier) AND (setting name OR identifier).</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        string GetTxtSetting(string userName, Guid? userGuid, string moduleName, Guid? moduleGuid, string settingName, Guid? settingGuid);

        /// <summary>
        /// Get value of <see cref="int" />-type setting by names of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>
        /// <para>Loading logic: user name AND module name AND setting name.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        string GetTxtSetting(string userName, string moduleName, string settingName);

        /// <summary>
        /// Get value of <see cref="int" />-type setting.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        string GetTxtSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid);

        /// <summary>
        /// Get value of common <see cref="int" />-type setting by name.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>
        /// <para>Loading logic: user identifier AND identifier name AND setting identifier.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        string GetCommonTxtSetting(string settingName);

        /// <summary>
        /// Get value of common <see cref="int" />-type setting by identifier.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        string GetCommonTxtSetting(Guid settingGuid);

        /// <summary>
        /// Get value of <see cref="int" />-type setting by names and identifiers of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>
        /// <para>Loading logic: (user name OR identifier) AND (module name OR identifier) AND (setting name OR identifier).</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        int? GetIntSetting(string userName, Guid? userGuid, string moduleName, Guid? moduleGuid, string settingName, Guid? settingGuid);

        /// <summary>
        /// Get value of <see cref="int" />-type setting by names of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>
        /// <para>Loading logic: user name AND module name AND setting name.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        int? GetIntSetting(string userName, string moduleName, string settingName);

        /// <summary>
        /// Get value of <see cref="int" />-type setting by identifiers of user and module.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>
        /// <para>Loading logic: user identifier AND identifier name AND setting identifier.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        int? GetIntSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid);

        /// <summary>
        /// Get value of common <see cref="int" />-type setting by name.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        int? GetCommonIntSetting(string settingName);

        /// <summary>
        /// Get value of common <see cref="int" />-type setting by identifier.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        int? GetCommonIntSetting(Guid settingGuid);

        /// <summary>
        /// Get value of <see cref="bool" />-type setting by names and identifiers of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>
        /// <para>Loading logic: (user name OR identifier) AND (module name OR identifier) AND (setting name OR identifier).</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        bool? GetBoolSetting(string userName, Guid? userGuid, string moduleName, Guid? moduleGuid, string settingName, Guid? settingGuid);

        /// <summary>
        /// Get value of <see cref="bool" />-type setting by names of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>
        /// <para>Loading logic: user name AND module name AND setting name.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        bool? GetBoolSetting(string userName, string moduleName, string settingName);

        /// <summary>
        /// Get value of <see cref="bool" />-type setting by identifiers of user and module.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>
        /// <para>Loading logic: user identifier AND identifier name AND setting identifier.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        bool? GetBoolSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid);

        /// <summary>
        /// Get value of common <see cref="bool" />-type setting by name.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        bool? GetCommonBoolSetting(string settingName);

        /// <summary>
        /// Get value of common <see cref="bool" />-type setting by identifier.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        bool? GetCommonBoolSetting(Guid settingGuid);

        /// <summary>
        /// Get value of <see cref="string" />-type setting by names and identifiers of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>
        /// <para>Loading logic: (user name OR identifier) AND (module name OR identifier) AND (setting name OR identifier).</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        Guid? GetGuidSetting(string userName, Guid? userGuid, string moduleName, Guid? moduleGuid, string settingName, Guid? settingGuid);

        /// <summary>
        /// Get value of <see cref="string" />-type setting by names of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>
        /// <para>Loading logic: user name AND module name AND setting name.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        Guid? GetGuidSetting(string userName, string moduleName, string settingName);

        /// <summary>
        /// Get value of <see cref="string" />-type setting by identifiers of user and module.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>
        /// <para>Loading logic: user identifier AND identifier name AND setting identifier.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        Guid? GetGuidSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid);

        /// <summary>
        /// Get value of common <see cref="string" />-type setting by name.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        Guid? GetCommonGuidSetting(string settingName);

        /// <summary>
        /// Get value of common <see cref="string" />-type setting by identifier.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        Guid? GetCommonGuidSetting(Guid settingGuid);

        /// <summary>
        /// Get value of <see cref="Decimal" />-type setting by names and identifiers of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>
        /// <para>Loading logic: (user name OR identifier) AND (module name OR identifier) AND (setting name OR identifier).</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        Decimal? GetDecimalSetting(string userName, Guid? userGuid, string moduleName, Guid? moduleGuid, string settingName, Guid? settingGuid);

        /// <summary>
        /// Get value of <see cref="Decimal" />-type setting by names of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>
        /// <para>Loading logic: user name AND module name AND setting name.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        Decimal? GetDecimalSetting(string userName, string moduleName, string settingName);

        /// <summary>
        /// Get value of <see cref="Decimal" />-type setting by identifiers of user and module.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>
        /// <para>Loading logic: user identifier AND identifier name AND setting identifier.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        Decimal? GetDecimalSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid);

        /// <summary>
        /// Get value of common <see cref="Decimal" />-type setting by name.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        Decimal? GetCommonDecimalSetting(string settingName);

        /// <summary>
        /// Get value of common <see cref="Decimal" />-type setting by identifier.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        Decimal? GetCommonDecimalSetting(Guid settingGuid);

        /// <summary>
        /// Get value of <see cref="DateTime" />-type setting by names and identifiers of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>
        /// <para>Loading logic: (user name OR identifier) AND (module name OR identifier) AND (setting name OR identifier).</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        DateTime? GetDateTimeSetting(string userName, Guid? userGuid, string moduleName, Guid? moduleGuid, string settingName, Guid? settingGuid);

        /// <summary>
        /// Get value of <see cref="DateTime" />-type setting by names of user and module.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>
        /// <para>Loading logic: user name AND module name AND setting name.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        DateTime? GetDateTimeSetting(string userName, string moduleName, string settingName);

        /// <summary>
        /// Get value of <see cref="DateTime" />-type setting by identifiers of user and module.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>
        /// <para>Loading logic: user identifier AND identifier name AND setting identifier.</para>
        /// <para>In case of multiple results only first record will be loaded. Others will be deleted.</para>
        /// </remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        DateTime? GetDateTimeSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid);

        /// <summary>
        /// Get value of common <see cref="DateTime" />-type setting by name.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        DateTime? GetCommonDateTimeSetting(string settingName);

        /// <summary>
        /// Get value of common <see cref="DateTime" />-type setting by identifier.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be loaded using specified arguments.</exception>
        /// <remarks>In case of multiple results only first record will be loaded. Others will be deleted.</remarks>
        /// <returns>Value of setting or <c>null</c> if it's not found or loading is disabled (<see cref="UseSettings"/> is <c>false</c>).</returns>
        DateTime? GetCommonDateTimeSetting(Guid settingGuid);

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
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetSettings(
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
            DateTime? dateTimeValue);

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
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetSettings(
            string userName,
            string moduleName,
            string settingName,
            string stringValue,
            string textValue,
            int? intValue,
            bool? boolValue,
            Guid? guidValue,
            decimal? decimalValue,
            DateTime? dateTimeValue);

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
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetSettings(
            Guid userGuid,
            Guid moduleGuid,
            Guid settingGuid,
            string stringValue,
            string textValue,
            int? intValue,
            bool? boolValue,
            Guid? guidValue,
            decimal? decimalValue,
            DateTime? dateTimeValue);
        
        #endregion
        
        #region Saving typed
        
        /// <summary>
        /// Set <see cref="string" />-type setting.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="stringValue">Value of <see cref="string" />-type setting with bounded length.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetStrSetting(string userName, string moduleName, string settingName, string stringValue);

        /// <summary>
        /// Set <see cref="string" />-type setting.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="stringValue">Value of <see cref="string" />-type setting with bounded length.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetStrSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid, string stringValue);

        /// <summary>
        /// Set common <see cref="string" />-type setting.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="stringValue">Value of <see cref="string" />-type setting with bounded length.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetCommonStrSetting(string settingName, string stringValue);

        /// <summary>
        /// Set common <see cref="string" />-type setting.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="stringValue">Value of <see cref="string" />-type setting with bounded length.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetCommonStrSetting(Guid settingGuid, string stringValue);

        /// <summary>
        /// Set <see cref="string" />-type setting with unbounded length.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="textValue">Value of <see cref="string" />-type setting with unbounded length.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetTxtSetting(string userName, string moduleName, string settingName, string textValue);

        /// <summary>
        /// Set <see cref="string" />-type setting with unbounded length.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="textValue">Value of <see cref="string" />-type setting with unbounded length.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetTxtSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid, string textValue);

        /// <summary>
        /// Set common <see cref="string" />-type setting with unbounded length.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="textValue">Value of <see cref="string" />-type setting with unbounded length.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetCommonTxtSetting(string settingName, string textValue);

        /// <summary>
        /// Set common <see cref="string" />-type setting with unbounded length.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="textValue">Value of <see cref="string" />-type setting with unbounded length.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetCommonTxtSetting(Guid settingGuid, string textValue);

        /// <summary>
        /// Set <see cref="int" />-type setting.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="intValue">Value of <see cref="int" />-type setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetIntSetting(string userName, string moduleName, string settingName, int intValue);

        /// <summary>
        /// Set <see cref="int" />-type setting.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="intValue">Value of <see cref="int" />-type setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetIntSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid, int intValue);

        /// <summary>
        /// Set common <see cref="int" />-type setting.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="intValue">Value of <see cref="int" />-type setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetCommonIntSetting(string settingName, int intValue);

        /// <summary>
        /// Set common <see cref="int" />-type setting.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="intValue">Value of <see cref="int" />-type setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetCommonIntSetting(Guid settingGuid, int intValue);

        /// <summary>
        /// Set <see cref="bool" />-type setting.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="boolValue">Value of <see cref="bool" />-type setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetBoolSetting(string userName, string moduleName, string settingName, bool boolValue);

        /// <summary>
        /// Set <see cref="bool" />-type setting.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="boolValue">Value of <see cref="bool" />-type setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetBoolSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid, bool boolValue);

        /// <summary>
        /// Set common <see cref="bool" />-type setting.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="boolValue">Value of <see cref="bool" />-type setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetCommonBoolSetting(string settingName, bool boolValue);

        /// <summary>
        /// Set common <see cref="bool" />-type setting.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="boolValue">Value of <see cref="bool" />-type setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetCommonBoolSetting(Guid settingGuid, bool boolValue);

        /// <summary>
        /// Set <see cref="string" />-type setting.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="guidValue">Value of <see cref="Guid" />-type setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetGuidSetting(string userName, string moduleName, string settingName, Guid guidValue);

        /// <summary>
        /// Set <see cref="string" />-type setting.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="guidValue">Value of <see cref="Guid" />-type setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetGuidSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid, Guid guidValue);

        /// <summary>
        /// Set common <see cref="string" />-type setting.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="guidValue">Value of <see cref="Guid" />-type setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetCommonGuidSetting(string settingName, Guid guidValue);

        /// <summary>
        /// Set common <see cref="string" />-type setting.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="guidValue">Value of <see cref="Guid" />-type setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetCommonGuidSetting(Guid settingGuid, Guid guidValue);

        /// <summary>
        /// Set <see cref="Decimal" />-type setting.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="decimalValue">Value of <see cref="decimal" />-type setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetDecimalSetting(string userName, string moduleName, string settingName, Decimal decimalValue);

        /// <summary>
        /// Set <see cref="Decimal" />-type setting.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="decimalValue">Value of <see cref="decimal" />-type setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetDecimalSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid, Decimal decimalValue);

        /// <summary>
        /// Set common <see cref="Decimal" />-type setting.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="decimalValue">Value of <see cref="decimal" />-type setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetCommonDecimalSetting(string settingName, Decimal decimalValue);

        /// <summary>
        /// Set common <see cref="Decimal" />-type setting.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="decimalValue">Value of <see cref="decimal" />-type setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetCommonDecimalSetting(Guid settingGuid, Decimal decimalValue);

        /// <summary>
        /// Установить <see cref="DateTime" />-type setting.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="dateTimeValue">Value of <see cref="DateTime" />-type setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetDateTimeSetting(string userName, string moduleName, string settingName, DateTime dateTimeValue);

        /// <summary>
        /// Установить <see cref="DateTime" />-type setting.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="dateTimeValue">Value of <see cref="DateTime" />-type setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetDateTimeSetting(Guid userGuid, Guid moduleGuid, Guid settingGuid, DateTime dateTimeValue);

        /// <summary>
        /// Set common <see cref="DateTime" />-type setting.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="dateTimeValue">Value of <see cref="DateTime" />-type setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetCommonDateTimeSetting(string settingName, DateTime dateTimeValue);

        /// <summary>
        /// Set common <see cref="DateTime" />-type setting.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <param name="dateTimeValue">Value of <see cref="DateTime" />-type setting.</param>
        /// <exception cref="ArgumentException">Thrown when setting value cannot be saved using specified arguments.</exception>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool SetCommonDateTimeSetting(Guid settingGuid, DateTime dateTimeValue);

        #endregion

        #region Deleting

        /// <summary>
        /// Delete all settings by user name and identifier.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <remarks>Deletes data using user name OR identifier.</remarks>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool DeleteSettingsByUser(string userName, Guid? userGuid);

        /// <summary>
        /// Delete all settings by user name.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool DeleteSettingsByUser(string userName);

        /// <summary>
        /// Delete all settings by user identifier.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool DeleteSettingsByUser(Guid userGuid);

        /// <summary>
        /// Delete all settings by module name or identifier.
        /// </summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <remarks >Deletes data using module name OR identifier.</remarks>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool DeleteSettingsByModule(string moduleName, Guid? moduleGuid);

        /// <summary>
        /// Delete settings.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <remarks>Deletes all settings usin OR logic for non-null arguments.</remarks>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool DeleteSettings(string userName, Guid? userGuid, string moduleName, Guid? moduleGuid, string settingName, Guid? settingGuid);

        /// <summary>
        /// Delete all settings by module and setting name or identifier.
        /// </summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool DeleteSettings(string moduleName, Guid? moduleGuid, string settingName, Guid? settingGuid);

        /// <summary>
        /// Delete setting by names.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool DeleteSettings(string userName, string moduleName, string settingName);

        /// <summary>
        /// Delete all settings.
        /// </summary>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool DeleteSettings();

        /// <summary>
        /// Delete setting by identifiers.
        /// </summary>
        /// <param name="userGuid">Identifier of the user.</param>
        /// <param name="moduleGuid">Identifier of the module.</param>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool DeleteSettings(Guid userGuid, Guid moduleGuid, Guid settingGuid);

        /// <summary>
        /// Delete all common settings.
        /// </summary>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool DeleteAllCommonSettings();

        /// <summary>
        /// Delete common setting by name.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool DeleteCommonSettings(string settingName);

        /// <summary>
        /// Delete common setting by identifier.
        /// </summary>
        /// <param name="settingGuid">Identifier of the setting.</param>
        /// <returns>Returns <c>true</c> on success.</returns>
        bool DeleteCommonSettings(Guid settingGuid);

        #endregion
    }
}
