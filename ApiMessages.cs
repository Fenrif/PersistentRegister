namespace PersistentRegister
{
    public static class ErrorMessages
    {
        public const string UserNotFound = "User not found.";
        public const string EmailExists = "Email {0} already exists.";
        public const string RegisterError = "Error registering user.";
        public const string SavingJson = "Error saving data to JSON file";
        public const string HttpPostRetry = "Error: {0} --- Retry: {1}";
    }

    public static class SuccessMessages
    {
        public const string UserInserted = "User inserted successfully.";
        public const string UserDeleted = "User deleted successfully.";
        public const string UserUpdated = "User updated successfully.";
        public const string UsersRetrieved = "Users retrieved successfully.";
    }

    public static class InformationMessages
    {
        public const string NoUsersFound = "No users found.";
    }
}