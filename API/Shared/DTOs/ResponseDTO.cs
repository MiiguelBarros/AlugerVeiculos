namespace API.Shared.DTOs
{
    public class ResponseDTO<T>
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public T? Data { get; set; }

        public int StatusCode { get; set; }

        public static ResponseDTO<T> Ok(string message, T data, int statusCode)
        {
            return new ResponseDTO<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = statusCode
            };
        }

        public static ResponseDTO<T> Ok(string message, int statusCode)
        {
            return new ResponseDTO<T>
            {
                Success = true,
                Message = message,
                Data = default,
                StatusCode = statusCode
            };
        }

        public static ResponseDTO<T> Fail(string message, T data, int statusCode)
        {
            return new ResponseDTO<T>
            {
                Success = false,
                Message = message,
                Data = data,
                StatusCode = statusCode
            };
        }
        public static ResponseDTO<T> Fail(string message, int statusCode)
        {
            return new ResponseDTO<T>
            {
                Success = false,
                Message = message,
                Data = default,
                StatusCode = statusCode
            };
        }
    }
}
