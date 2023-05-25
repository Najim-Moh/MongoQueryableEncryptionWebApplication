using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;



namespace MongoCSFLEWebApplication.Models
{
    class YourCredentials
    {


        //Path to atlas and Shared-library 
        private Dictionary<string, string> credentials = new Dictionary<string, string>()
            {
                // Mongo Paths + URI
                {"MONGODB_URI", "mongodb+srv://Najim:Asakuki1@cluster0.uqvdtgc.mongodb.net/?retryWrites=true&w=majority"},
                {"SHARED_LIB_PATH", "/app/Mongo_Crypt_Linux/lib/mongo_crypt_v1.so"},
             };

        private void CheckThatValuesAreSet()
        {
            var placeholder = new Regex("^<.*>$");
            var errorBuffer = new List<String>();
            foreach (KeyValuePair<string, string> entry in credentials)
            {
                if (entry.Value != null && placeholder.IsMatch(Convert.ToString(entry.Value)))
                {
                    var message = String.Format("You must fill out the {0} field of your credentials object.", entry.Key);
                    errorBuffer.Add(message);
                }
            }
            if (errorBuffer.Count > 0)
            {
                var message = String.Join("\n", errorBuffer);
                throw new Exception(message);
            }
        }

        public Dictionary<string, string> GetCredentials()
        {
            CheckThatValuesAreSet();
            return credentials;
        }

    }
}
