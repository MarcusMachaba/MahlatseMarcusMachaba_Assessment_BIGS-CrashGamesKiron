using System;
using System.Collections.Generic;

namespace Core.ModelBase.Models
{
    public class WebData
    {
        public WebData()
        {
            ErrorDetails = new List<string>();
        }
        public bool Successful { get; set; }
        public string InformationMessage { get; set; }
        public string ErrorMessage { get; set; }
        public List<string> ErrorDetails { get; set; }

        public void SetErrors(Exception e)
        {
            if (e == null) return;
            ErrorMessage = e.Message;
            Successful = false;
            var exe = e;
            while (exe != null)
            {
                ErrorDetails.AddRange(new[] { e.Message, e.StackTrace });
                exe = e.InnerException;
            }
        }

        public void AddError(string errorMessage)
        {
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                Successful = false;
                ErrorMessage = errorMessage;
            }
        }
        public static WebData Success(string message = "Successfully Executed")
        {
            return new WebData { Successful = true, InformationMessage = message };
        }
    }
}
