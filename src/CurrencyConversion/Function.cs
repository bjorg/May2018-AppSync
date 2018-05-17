using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace CurrencyConversion {

    public class OrderDetails {

        //--- Fields ---
        public int Quantity;
        public double UnitPrice;
        public double Discount;
    }

    public class Conversion {

        //--- Fields ---
        public string currency;
    }

    public class Request {

        //--- Fields ---
        public OrderDetails[] source;
        public Conversion arguments;
    }

    public class Function {

        //--- Class Fields ---
        public static HttpClient HttpClient = new HttpClient();

        //--- Methods ---
        public async Task<double> FunctionHandlerAsync(Request input, ILambdaContext context) {
            LambdaLogger.Log($"*** INFO: invocation = {JsonConvert.SerializeObject(input)}");
            double rate = 1.0;
            var key = $"USD_{input.arguments.currency ?? "USD"}";
            if((input.arguments.currency != null) && (input.arguments.currency != "USD")) {
                using(var response = await HttpClient.SendAsync(new HttpRequestMessage {
                    RequestUri =  new Uri($"http://free.currencyconverterapi.com/api/v5/convert?q={key}&compact=y"),
                    Method = HttpMethod.Get
                })) {
                    rate = (double)JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync())[key].val;
                }
            }
            var result = input.source.Sum(item => (item.Quantity * item.UnitPrice * rate) - item.Discount);
            LambdaLogger.Log($"*** INFO: result = {JsonConvert.SerializeObject(result)}");
            return result;
        }
    }
}
