using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QRCoder;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace E_TAX_GHANA
{
    public partial class Form1 : Form
    {
        private object formattedMessage;

        public Form1()
        {
            InitializeComponent();
        }

        public class SdcInformation
        {
            public string SdcId { get; set; }
            public int ItemCount { get; set; }
            public int ReceiptNumber { get; set; }
            public DateTime ReceiptDateTime { get; set; }
            public string Mrc { get; set; }
            public string InternalData { get; set; }
            public string ReceiptSignature { get; set; }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            // Your JSON data
            string json = @"
            {
                ""currency"": ""GHS"",
                ""exchangeRate"": 1.0,
                ""invoiceNumber"": ""84655"",
                ""totalLevy"": 63.99,
                ""userName"": ""Kofi Ghana"",
                ""flag"": ""INVOICE"",
                ""calculationType"": ""INCLUSIVE"",
                ""totalVat"": 169.57,
                ""transactionDate"": ""2023-07-24T08:43:28Z"",
                ""totalAmount"": 1300.0,
                ""voucherAmount"": 0.0,
                ""businessPartnerName"": ""fred(cash customer)"",
                ""businessPartnerTin"": ""C0000000000"",
                ""saleType"": ""NORMAL"",
                ""discountType"": ""GENERAL"",
                ""discountAmount"": 0.0,
                ""reference"": """",
                ""groupReferenceId"": """",
                ""purchaseOrderReference"": """",
                ""items"": [
                    {
                        ""itemCode"": ""TXC00389165855"",
                        ""itemCategory"": """",
                        ""expireDate"": ""2025-01-01"",
                        ""description"": ""1SFA611130R1101"",
                        ""quantity"": 10.0,
                        ""levyAmountA"": 26.66,
                        ""levyAmountB"": 26.66,
                        ""levyAmountC"": 10.66,
                        ""levyAmountD"": 0.0,
                        ""levyAmountE"": 0.0,
                        ""discountAmount"": 0.0,
                        ""batchCode"": """",
                        ""unitPrice"": 130.0
                    }

                ]
            }";

            string url = "https://vsdcstaging.vat-gh.com/vsdc/api/v1/taxpayer/CXX000000YY-001/invoice"; // Update with your server URL
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json";

                string securityKey = "Z60gftKe9sei3xOZhvvDa0StkVILKR3j5MBM9ygi1zg=";
                request.Headers.Add("security_key", securityKey);

                using (var writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(json);
                }

                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    // Extract the status code
                    int statusCode = (int)response.StatusCode;

                    if (statusCode == 200)
                    {
                        textBox1.Text = statusCode.ToString();
                        textBox3.Text = "";
                        textBox2.Text = "";
                    }
                    else
                    {
                        textBox1.Text = statusCode.ToString();
                        textBox3.Text = "Error: " + response.StatusDescription;
                        textBox2.Text = "";
                    }

                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        var result = reader.ReadToEnd();

                        // Parse the JSON response
                        var jsonResponse = JObject.Parse(result);

                        // Format the JSON response without quotation marks and curly brackets
                        var formattedJson = JsonConvert.SerializeObject(jsonResponse, Formatting.Indented);
                        formattedJson = formattedJson.Replace("{", "").Replace("}", "").Replace("\"", "");

                        // Display the formatted JSON response in textBox2
                        textBox2.Text = formattedJson;

                        var sdcInfo = JsonConvert.DeserializeObject<SdcInformation>(result);

                        // Display the SDC INFORMATION
                        textBox2.Text = $"SDC ID: {sdcInfo.SdcId}\r\n";
                        textBox2.Text += $"ITEM COUNT: {sdcInfo.ItemCount}\r\n";
                        textBox2.Text += $"RECEIPT NUMBER: {sdcInfo.ReceiptNumber}\r\n";
                        textBox2.Text += $"RECEIPT DATE & TIME: {sdcInfo.ReceiptDateTime.ToString("dddd, MMMM dd, yyyy h:mm tt")}\r\n";
                        textBox2.Text += $"MRC: {sdcInfo.Mrc}\r\n";
                        textBox2.Text += $"INTERNAL DATA: {sdcInfo.InternalData}\r\n";
                        textBox2.Text += $"RECEIPT SIGNATURE: {sdcInfo.ReceiptSignature}\r\n";

                        // Generate a QR code
                        var qrGenerator = new QRCodeGenerator();
                        var qrCodeData = qrGenerator.CreateQrCode(JsonConvert.SerializeObject(formattedMessage), QRCodeGenerator.ECCLevel.Q);
                        var qrCode = new PngByteQRCode(qrCodeData);
                        var qrCodeBytes = qrCode.GetGraphic(10); // You can adjust the size here

                        // Convert bytes to an image and display it
                        using (var stream = new MemoryStream(qrCodeBytes))
                        {
                            pictureBox1.Image = System.Drawing.Image.FromStream(stream);
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse errorResponse)
                {
                    // Handle the exception and display the numeric status code
                    int statusCode = (int)errorResponse.StatusCode;
                    textBox1.Text = statusCode.ToString();
                    textBox2.Text = "";
                    textBox3.Text = "Error: " + ex.Message;
                }
                else
                {
                    // Handle other exceptions
                    textBox1.Text = "";
                    textBox2.Text = "";
                    textBox3.Text = "Error: " + ex.Message;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // JSON data for the purchase request
            string jsonPurchase = @"
            {
                ""currency"": ""GHS"",
                ""exchangeRate"": 1.0,
                ""invoiceNumber"": ""788784565"",
                ""totalLevy"": 63.99,
                ""userName"": ""Kofi Ghana"",
                ""flag"": ""PURCHASE"",
                ""calculationType"": ""INCLUSIVE"",
                ""totalVat"": 169.57,
                ""transactionDate"": ""2023-07-24T08:43:28Z"",
                ""totalAmount"": 1300.0,
                ""voucherAmount"": 0.0,
                ""businessPartnerName"": ""fred(cash customer)"",
                ""businessPartnerTin"": ""C0000000000"",
                ""saleType"": ""NORMAL"",
                ""discountType"": ""GENERAL"",
                ""discountAmount"": 0.0,
                ""reference"": """",
                ""groupReferenceId"": """",
                ""purchaseOrderReference"": """",
                ""items"": [
                    {
                        ""itemCode"": ""item"",
                        ""itemCategory"": """",
                        ""expireDate"": ""2025-01-01"",
                        ""description"": ""desc"",
                        ""quantity"": ""qty"",
                        ""levyAmountA"": 26.66,
                        ""levyAmountB"": 26.66,
                        ""levyAmountC"": 10.66,
                        ""levyAmountD"": 0.0,
                        ""levyAmountE"": 0.0,
                        ""discountAmount"": 0.0,
                        ""batchCode"": """",
                        ""unitPrice"": 130.0
                    }
               ]
            }";

            string urlPurchase = "https://vsdcstaging.vat-gh.com/vsdc/api/v1/taxpayer/CXX000000YY-001/invoice";
            try
            {
                var requestPurchase = (HttpWebRequest)WebRequest.Create(urlPurchase);
                requestPurchase.Method = "POST";
                requestPurchase.ContentType = "application/json";

                string securityKey = "Z60gftKe9sei3xOZhvvDa0StkVILKR3j5MBM9ygi1zg=";
                requestPurchase.Headers.Add("security_key", securityKey);

                using (var writer = new StreamWriter(requestPurchase.GetRequestStream()))
                {
                    writer.Write(jsonPurchase);
                }

                using (var responsePurchase = requestPurchase.GetResponse() as HttpWebResponse)
                {
                    // Extract the status code
                    int statusCode = (int)responsePurchase.StatusCode;

                    if (statusCode == 200)
                    {
                        // Handle success
                        textBox1.Text = statusCode.ToString();
                        textBox3.Text = "";
                        textBox2.Text = "";
                    }
                    else
                    {
                        // Handle failure
                        textBox1.Text = statusCode.ToString();
                        textBox3.Text = "Error: " + responsePurchase.StatusDescription;
                        textBox2.Text = "";
                    }
                    // You can also handle the response JSON if needed
                    using (var reader = new StreamReader(responsePurchase.GetResponseStream()))
                    {
                        var result = reader.ReadToEnd();
                        // Parse and handle the purchase response JSON as required

                        // Display the formatted JSON response in textBox2
                        var jsonResponse = JObject.Parse(result);
                        var formattedJson = JsonConvert.SerializeObject(jsonResponse, Formatting.Indented);
                        formattedJson = formattedJson.Replace("{", "").Replace("}", "").Replace("\"", "");

                        textBox2.Text = formattedJson;

                        // Generate a QR code
                        var qrGenerator = new QRCodeGenerator();
                        var qrCodeData = qrGenerator.CreateQrCode(formattedJson, QRCodeGenerator.ECCLevel.Q);
                        var qrCode = new PngByteQRCode(qrCodeData);
                        var qrCodeBytes = qrCode.GetGraphic(10); // You can adjust the size here

                        // Convert bytes to an image and display it
                        using (var stream = new MemoryStream(qrCodeBytes))
                        {
                            pictureBox1.Image = System.Drawing.Image.FromStream(stream);
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse errorResponse)
                {
                    // Handle the exception and display the numeric status code
                    int statusCode = (int)errorResponse.StatusCode;
                    textBox1.Text = statusCode.ToString();
                    textBox2.Text = "";
                    textBox3.Text = "Error: " + ex.Message;
                }
                else
                {
                    // Handle other exceptions
                    textBox1.Text = "";
                    textBox2.Text = "";
                    textBox3.Text = "Error: " + ex.Message;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // JSON data for the purchase return request
            string jsonPurchaseReturn = @"
            {
                ""currency"": ""GHS"",
                ""exchangeRate"": 1.0,
                ""invoiceNumber"": ""8465445"",
                ""totalLevy"": 63.99,
                ""userName"": ""Kofi Ghana"",
                ""flag"": ""PURCHASE_RETURN"",
                ""calculationType"": ""INCLUSIVE"",
                ""totalVat"": 169.57,
                ""transactionDate"": ""2023-07-24T08:43:28Z"",
                ""totalAmount"": 1300.0,
                ""voucherAmount"": 0.0,
                ""businessPartnerName"": ""fred(cash customer)"",
                ""businessPartnerTin"": ""C0000000000"",
                ""saleType"": ""NORMAL"",
                ""discountType"": ""GENERAL"",
                ""discountAmount"": 0.0,
                ""reference"": """",
                ""groupReferenceId"": """",
                ""items"": [
                    {
                        ""itemCode"": ""item"",
                        ""itemCategory"": """",
                        ""expireDate"": ""2025-01-01"",
                        ""description"": ""desc"",
                        ""quantity"": ""qty"",
                        ""levyAmountA"": 26.66,
                        ""levyAmountB"": 26.66,
                        ""levyAmountC"": 10.66,
                        ""levyAmountD"": 0.0,
                        ""levyAmountE"": 0.0,
                        ""discountAmount"": 0.0,
                        ""batchCode"": """",
                        ""unitPrice"": 130.0
                    }
               ]
            }";

            string url = "https://vsdcstaging.vat-gh.com/vsdc/api/v1/taxpayer/CXX000000YY-001/invoice"; // Update with your server URL
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json";

                string securityKey = "Z60gftKe9sei3xOZhvvDa0StkVILKR3j5MBM9ygi1zg=";
                request.Headers.Add("security_key", securityKey);

                using (var writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(jsonPurchaseReturn);
                }

                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    // Extract the status code
                    int statusCode = (int)response.StatusCode;

                    if (statusCode == 200)
                    {
                        textBox1.Text = statusCode.ToString();
                        textBox3.Text = "";
                        textBox2.Text = "";
                    }
                    else
                    {
                        textBox1.Text = statusCode.ToString();
                        textBox3.Text = "Error: " + response.StatusDescription;
                        textBox2.Text = "";
                    }

                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        var result = reader.ReadToEnd();

                        // Parse the JSON response
                        var jsonResponse = JObject.Parse(result);

                        // Format the JSON response without quotation marks and curly brackets
                        var formattedJson = JsonConvert.SerializeObject(jsonResponse, Formatting.Indented);
                        formattedJson = formattedJson.Replace("{", "").Replace("}", "").Replace("\"", "");

                        // Display the formatted JSON response in textBox2
                        textBox2.Text = formattedJson;

                        var sdcInfo = JsonConvert.DeserializeObject<SdcInformation>(result);

                        // Display the SDC INFORMATION
                        textBox2.Text = $"SDC ID: {sdcInfo.SdcId}\r\n";
                        textBox2.Text += $"ITEM COUNT: {sdcInfo.ItemCount}\r\n";
                        textBox2.Text += $"RECEIPT NUMBER: {sdcInfo.ReceiptNumber}\r\n";
                        textBox2.Text += $"RECEIPT DATE & TIME: {sdcInfo.ReceiptDateTime.ToString("dddd, MMMM dd, yyyy h:mm tt")}\r\n";
                        textBox2.Text += $"MRC: {sdcInfo.Mrc}\r\n";
                        textBox2.Text += $"INTERNAL DATA: {sdcInfo.InternalData}\r\n";
                        textBox2.Text += $"RECEIPT SIGNATURE: {sdcInfo.ReceiptSignature}\r\n";

                        // Generate a QR code
                        var qrGenerator = new QRCodeGenerator();
                        var qrCodeData = qrGenerator.CreateQrCode(JsonConvert.SerializeObject(formattedMessage), QRCodeGenerator.ECCLevel.Q);
                        var qrCode = new PngByteQRCode(qrCodeData);
                        var qrCodeBytes = qrCode.GetGraphic(10); // You can adjust the size here

                        // Convert bytes to an image and display it
                        using (var stream = new MemoryStream(qrCodeBytes))
                        {
                            pictureBox1.Image = System.Drawing.Image.FromStream(stream);
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse errorResponse)
                {
                    // Handle the exception and display the numeric status code
                    int statusCode = (int)errorResponse.StatusCode;
                    textBox1.Text = statusCode.ToString();
                    textBox2.Text = "";
                    textBox3.Text = "Error: " + ex.Message;
                }
                else
                {
                    // Handle other exceptions
                    textBox1.Text = "";
                    textBox2.Text = "";
                    textBox3.Text = "Error: " + ex.Message;
                }
            }
        }



        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        // JSON data for the refund request
        //string jsonRefund = @"
        /* {
            ""currency"": ""GHS"",
            ""exchangeRate"": 1.0,
            ""invoiceNumber"": ""987654"",
            ""totalLevy"": 30.00,
            ""userName"": ""Kofi Ghana"",
            ""flag"": ""REFUND"",
            ""totalVat"": 79.50,
            ""transactionDate"": ""2023-07-24T09:43:22"",
            ""totalAmount"": 609.50,
            ""voucherAmount"": 0.0,
            ""businessPartnerName"": ""fred(cash customer)"",
            ""businessPartnerTin"": ""C0000000000"",
            ""saleType"": ""NORMAL"",
            ""discountType"": ""GENERAL"",
            ""discountAmount"": 0.00,
            ""reference"": ""56477"",
            ""groupReferenceId"": """",
            ""items"": [
                 {
                     ""itemCode"": ""TXC00271018229"",
                     ""itemCategory"": """",
                     ""expireDate"": null,
                     ""description"": ""A27040652-WHEEL CYLINDER MAJOR"", desc
                     ""quantity"": 1.00000,
                     ""levyAmountA"": 12.50,
                     ""levyAmountB"": 12.50,
                     ""levyAmountC"": 5.00,
                     ""levyAmountD"": 0.00,
                     ""levyAmountE"": 0.00,
                     ""discountAmount"": 0.00,
                     ""taxCode"": ""B"",
                     ""batchCode"": """",
                     ""unitPrice"": 609.5,
                     ""taxRate"": 15.00
                 }
            ]
         }";*/

        // string urlRefund = "https://vsdcstaging.vat-gh.com/vsdc/api/v1/taxpayer/CXX000000YY-001/refund"; // Update with the refund endpoint

        /* try 
         {
             var requestRefund = (HttpWebRequest)WebRequest.Create(urlRefund);
             requestRefund.Method = "POST";
             requestRefund.ContentType = "application/json";

             string securityKey = "Z60gftKe9sei3xOZhvvDa0StkVILKR3j5MBM9ygi1zg=";
             requestRefund.Headers.Add("security_key", securityKey);

             using (var writer = new StreamWriter(requestRefund.GetRequestStream()))
             {
                 writer.Write(jsonRefund);
             }

             using (var responseRefund = requestRefund.GetResponse() as HttpWebResponse)
             {
                 // Extract the status code
                 int statusCode = (int)responseRefund.StatusCode;

                 if (statusCode == 200)
                 {
                     // Handle success
                     textBox1.Text = statusCode.ToString();
                     textBox3.Text = "Refund request succeeded.";
                 }
                 else
                 {
                     // Handle failure
                     textBox1.Text = statusCode.ToString();
                     textBox3.Text = "Error: " + responseRefund.StatusDescription;
                     textBox2.Text = "";
                 }
                 // You can also handle the response JSON if needed
                 using (var reader = new StreamReader(responseRefund.GetResponseStream()))
                 {
                     var result = reader.ReadToEnd();
                     // Parse and handle the refund response JSON as required

                     // Display the formatted JSON response in textBox2
                     var jsonResponse = JObject.Parse(result);
                     var formattedJson = JsonConvert.SerializeObject(jsonResponse, Formatting.Indented);
                     formattedJson = formattedJson.Replace("{", "").Replace("}", "").Replace("\"", "");

                     textBox2.Text = formattedJson;

                     // Parse the JSON response into SdcInformation object
                     var sdcInfo = JsonConvert.DeserializeObject<SdcInformation>(result);

                     // Display the SDC INFORMATION
                     textBox2.Text = $"SDC ID: {sdcInfo.SdcId}\r\n";
                     textBox2.Text += $"ITEM COUNT: {sdcInfo.ItemCount}\r\n";
                     textBox2.Text += $"RECEIPT NUMBER: {sdcInfo.ReceiptNumber}\r\n";
                     textBox2.Text += $"RECEIPT DATE & TIME: {sdcInfo.ReceiptDateTime.ToString("dddd, MMMM dd, yyyy h:mm tt")}\r\n";
                     textBox2.Text += $"MRC: {sdcInfo.Mrc}\r\n";
                     textBox2.Text += $"INTERNAL DATA: {sdcInfo.InternalData}\r\n";
                     textBox2.Text += $"RECEIPT SIGNATURE: {sdcInfo.ReceiptSignature}\r\n";

                     // Generate a QR code
                     var qrGenerator = new QRCodeGenerator();
                     var qrCodeData = qrGenerator.CreateQrCode(JsonConvert.SerializeObject(sdcInfo), QRCodeGenerator.ECCLevel.Q);
                     var qrCode = new PngByteQRCode(qrCodeData);
                     var qrCodeBytes = qrCode.GetGraphic(10); // You can adjust the size here

                     // Convert bytes to an image and display it
                     using (var stream = new MemoryStream(qrCodeBytes))
                     {
                         pictureBox1.Image = System.Drawing.Image.FromStream(stream);
                     }
                 }
             }
         }
         catch (WebException ex)
         {
             if (ex.Response is HttpWebResponse errorResponse)
             {
                 // Handle the exception and display the numeric status code
                 int statusCode = (int)errorResponse.StatusCode;
                 textBox1.Text = statusCode.ToString();
                 textBox2.Text = "";
                 textBox3.Text = "Error: " + ex.Message;
             }
             else
             {
                 // Handle other exceptions
                 textBox1.Text = "";
                 textBox2.Text = "";
                 textBox3.Text = "Error: " + ex.Message;
             }
         }*/
    }
}
