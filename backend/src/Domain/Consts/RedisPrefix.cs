namespace Domain.Consts
{
    public static class RedisPrefix
    {
        public const string RedisTokenKeyPrefix = "auth:user-token:";
        public const string RedisTokenToUserKeyPrefix = "auth:token-user:"; // tokenHash -> userId
    }
}
