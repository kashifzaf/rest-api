namespace Ensek.Models {
    public class Result { 
        public int SuccessfulCount { get; set; }
        public int FailedCount { get; set; }
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }        
    }

    public class Result<TData> : Result {
        public TData Data { get; set; }
    }

}