namespace Game.Server.Core
{
    public class OperationResult
    {
        public bool IsSucceed { get; set; }
        public object Data { get; set; }
        public string ErrorMessage { get; set; }

        public static OperationResult Failure(string errorMessage, object data)
        {
            return new OperationResult { IsSucceed= false, Data =data, ErrorMessage = errorMessage };
        }

        public static OperationResult Succeed(object data)
        {
            return new OperationResult { IsSucceed = true, Data = data };
        }
    }
}
