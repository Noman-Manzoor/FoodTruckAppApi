namespace FoodTruckAppApi.DTOs
{
    public class OperationResult
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }
        public object Data { get; private set; }

        public OperationResult(bool success, string message, object data = null)
        {
            Success = success;
            Message = message;
            Data = data;
        }
    }
}
