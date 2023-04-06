namespace NewPlatform.Flexberry.ORM.ODataService.UserSettingsService
{
    using System;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using NewPlatform.Flexberry.ORM.CurrentUserService;

    /// <summary>
    /// Бизнес-сервер для класса <see cref="FlexberryUserSetting"/>.
    /// </summary>
    public class FlexberryUserSettingBS : BusinessServer
    {
        private ICurrentUser currentUser;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlexberryUserSettingBS"/> class.
        /// </summary>
        /// <param name="currentUser">Setting of mechanism of getting current user.</param>
        public FlexberryUserSettingBS(ICurrentUser currentUser)
        {
            string errorMessage = "Add resolving of interface ICurrentUser to dependency injection system. \r\n" +
                                  "If Unity, add to unity config something like: \r\n" +
                                  "<register type=\"NewPlatform.Flexberry.ORM.CurrentUserService.ICurrentUser, NewPlatform.Flexberry.ORM.CurrentUserService\" mapTo=\"NewPlatform.Flexberry.ORM.CurrentUserService.EmptyCurrentUser, NewPlatform.Flexberry.ORM.CurrentUserService\">\r\n" +
                                  "  <constructor />\r\n" +
                                  "</register> ";
            this.currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser), errorMessage);
        }

        /// <summary>
        /// Обработка изменения класса <see cref="FlexberryUserSetting"/>.
        /// В создаваемую запись записывается текущий пользователь.
        /// В редактируемой записи проверяется, что пользователь также соответствует текущему, в противном случае кидается исключение.
        /// </summary>
        /// <param name="updatedObject">Изменяемый объект.</param>
        /// <returns>Дополнительные объекты, которые требуется сохранить.</returns>
        /// <exception cref="System.Exception">Исключение кидается, если у редактируемой записи пользователь не соответствует текущему.</exception>
        public DataObject[] OnUpdateFlexberryUserSetting(FlexberryUserSetting updatedObject)
        {
            ObjectStatus objectStatus = updatedObject.GetStatus();
            ICurrentUser user = currentUser;

            string currentUserName = "Anonymous";
            if (user != null)
            {
                if (string.IsNullOrEmpty(user.Domain))
                {
                    currentUserName = user.Login;
                }
                else
                {
                    currentUserName = string.Concat(user.Domain, "\\", user.Login);
                }
            }

            if (objectStatus == ObjectStatus.Created)
            {
                updatedObject.UserName = currentUserName;
            }

            if (objectStatus == ObjectStatus.Altered && updatedObject.UserName != currentUserName && !(string.IsNullOrEmpty(updatedObject.UserName) && string.IsNullOrEmpty(currentUserName)))
            {
                throw new System.Exception("Altered user setting record doesn't belong to current user.");
            }

            return new DataObject[0];
        }
    }
}
