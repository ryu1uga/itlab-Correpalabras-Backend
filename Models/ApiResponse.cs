namespace CorrePalabras.Models.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }

        public ApiResponse(bool success, T data)
        {
            Success = success;
            Data = data;
        }

        // Métodos estáticos para facilitar el uso
        public static ApiResponse<T> Ok(T data) => new ApiResponse<T>(true, data);
        public static ApiResponse<string> Fail(string message) => new ApiResponse<string>(false, message);
    }
}