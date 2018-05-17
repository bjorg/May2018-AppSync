using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace CurrencyConversion {

    public class Request {

        //--- Fields ---
        public int Quantity;
        public double UnitPrice;
        public string Currency;
    }

    public class Function {

        //--- Class Fields ---
        public static HttpClient HttpClient = new HttpClient();

        //--- Methods ---
        public async Task<object> FunctionHandlerAsync(Request input, ILambdaContext context) {
            var key = $"USD_{input.Currency}";
            using(var response = await HttpClient.SendAsync(new HttpRequestMessage {
                RequestUri =  new Uri($"http://free.currencyconverterapi.com/api/v5/convert?q={key}&compact=y"),
                Method = HttpMethod.Get
            })) {
                var rate = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                return new {
                    Rate = rate,
                    Exchange = key,
                    Total = input.Quantity * input.UnitPrice * rate
                };
            }
        }
    }
}
