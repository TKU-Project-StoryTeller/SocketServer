using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SocketPratice
{
    public class GetAwsS3File
    {
        private const string bucketName = "useractions";
        private const string keyName = "upload/LeftTurn/point_";
        // Specify your bucket region (an example region is shown).
        private const string access = @"AKIAZV3YQDP4IY74OINM";
        private const string secret = @"Qhkoo22UsXJWW/kB7ErJnzRbxhqqhntDNX1dOUf2";
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USEast1;
        private static readonly AWSCredentials awsCredentials = new BasicAWSCredentials(access, secret);
        private static IAmazonS3 client;
        public string responseBody = "";

        public static bool isExist = true;
        public int num;

        public GetAwsS3File()
        {

            client = new AmazonS3Client(bucketRegion);
            num = 0;
        }
        public string getData()
        {
            string str = responseBody;
            num++;
            return str;
        }
        public bool FileIsExist()
        {
            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName + num + ".txt"
                };
                //Console.WriteLine(request.ToString());
                if (!client.GetObjectAsync(request).IsFaulted)//這裡有問題 沒辦法判定(?
                {
                    isExist = true;
                    ReadObjectDataAsync(request, num).Wait();
                    //getData();
                }
                else isExist = false;
                return isExist;
            }
            catch (AmazonS3Exception e)
            {
                return false;
            }


        }

        public async Task ReadObjectDataAsync(GetObjectRequest request, int num)
        {
            responseBody = "";
            try
            {

                isExist = true;
                using (GetObjectResponse response = await client.GetObjectAsync(request))
                using (Stream responseStream = response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string title = response.Metadata["x-amz-meta-title"]; // Assume you have "title" as medata added to the object.
                    string contentType = response.Headers["Content-Type"];
                    //Console.WriteLine("Object metadata, Title: {0}", title);
                    //Console.WriteLine("Content type: {0}", contentType);

                    responseBody = reader.ReadToEnd(); // Now you process the response body.
                                                       //Console.WriteLine();
                }



            }
            catch (AmazonS3Exception e)
            {
                // If bucket or object does not exist
                Console.WriteLine("Error encountered ***. Message:'{0}' when reading object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when reading object", e.Message);
            }
        }
    }
}