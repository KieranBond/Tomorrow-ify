namespace LambdaAnnotations;

internal class InvalidUserException(string userKey) : Exception($"Invalid user stored with {userKey}");
