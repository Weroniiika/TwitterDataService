using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

///<summary>
/// This class is used to implement all the Twitter requirements regarding authorization of requests. See <see cref="https://dev.twitter.com/docs/auth/authorizing-request"/> for more specific information.     
/// It used by TwitterPublicStreamRequest to obtain authorization string that is used in request header as "Authorization" value.
///</summary>
 
public class Authorization
{
    private static readonly string trackParam = "track";
    private static readonly string delimitedParam = "delimited";
    private static readonly string delimitedValue = "length";
    private static readonly string consumerKey = "oauth_consumer_key";
    private static readonly string consumerKeyValue = ""; //here you provide your consumer key value
    private static readonly string signatureMethod = "oauth_signature_method";
    private static readonly string signatureMethodValue = "HMAC-SHA1";
    private static readonly string token = "oauth_token";
    private static readonly string tokenValue = ""; //here you provide your application token value
    private static readonly string version = "oauth_version";
    private static readonly string versionValue = "1.0";
    private static readonly string nonce = "oauth_nonce";
    private static readonly string timestamp = "oauth_timestamp";
    private static readonly string signature = "oauth_signature";

    private static readonly string tokenSecretValue = ""; //here you provide your application token secret value
    private static readonly string consumerSecretValue = ""; //here you provide your consumer secret value

    ///<summary>
    ///This is main method that returns authorization string. It calls other helper methods to obtain proper fields values. The values are then used to create url-encoded authorization string according to Twitter directives
    ///</summary>
    public static string InitAuthValue(string httpMethodValue, string urlValue, string trackValue)
    {
        string timestampValue = InitTimestampValue();
        string nonceValue = InitNonceValue();
        string signatureBaseString = InitSignatureBaseString(timestampValue, nonceValue, trackValue, httpMethodValue, urlValue);
        string signatureValue = InitSignatureValue(signatureBaseString);

        StringBuilder authorizationBuilder = new StringBuilder();
        authorizationBuilder.Append("OAuth ");
        authorizationBuilder.Append(string.Concat(Uri.EscapeDataString(consumerKey), "=\"", Uri.EscapeDataString(consumerKeyValue), "\""));
        authorizationBuilder.Append(string.Concat(", ", Uri.EscapeDataString(nonce), "=\"", Uri.EscapeDataString(nonceValue),"\"" ));
        authorizationBuilder.Append(string.Concat(", ", Uri.EscapeDataString(signature), "=\"", Uri.EscapeDataString(signatureValue), "\""));
        authorizationBuilder.Append(string.Concat(", ", Uri.EscapeDataString(signatureMethod), "=\"", Uri.EscapeDataString(signatureMethodValue), "\""));
        authorizationBuilder.Append(string.Concat(", ", Uri.EscapeDataString(timestamp), "=\"", Uri.EscapeDataString(timestampValue), "\""));
        authorizationBuilder.Append(string.Concat(", ", Uri.EscapeDataString(token), "=\"", Uri.EscapeDataString(tokenValue), "\""));
        authorizationBuilder.Append(string.Concat(", ", Uri.EscapeDataString(version), "=\"", Uri.EscapeDataString(versionValue), "\""));
            
        return authorizationBuilder.ToString();
    }
    ///<summary>
    ///This is helper method that returns unique nonce value as required by Twitter
    ///</summary>
    private static string InitNonceValue()
    {
        return Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture)));
    }
    ///<summary>
    /// This is helper method that returns TimeStamp value.     
    ///</summary>
    private static string InitTimestampValue()
    {
       TimeSpan timeSpan = new TimeSpan();
       timeSpan= DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0); 
 
       return Convert.ToInt64(timeSpan.TotalSeconds).ToString(CultureInfo.InvariantCulture);
    }

    ///<summary>
    /// This is helper method that returns required by Twitter signature value. The signature value is created by hashing signatureBaseString with use of HMACSHA1 hashing algorithm and signature key. 
    /// Signature key is obtained by creating URI from consumerSecretValue field and tokenSecretValue field. 
    /// SignatureBaseString is a string consisting of all the parameters and its values that are used in the request.
    /// See <see cref="https://dev.twitter.com/docs/auth/creating-signature"/> for more specific information.
    ///</summary>

    public static string InitSignatureBaseString(string timestampValue, string nonceValue, string trackValue, string httpMethodValue, string urlValue)
    {
        StringBuilder parameterBuilder = new StringBuilder();

        Dictionary<string, string> oauthKeyValuePairs = new Dictionary<string, string> 
         {
           // {includeEntities, includeEntitiesValue},
            {delimitedParam,delimitedValue},
   //         {languageParam, languageValue},
            {consumerKey, consumerKeyValue},
            {nonce, nonceValue},
            {signatureMethod, signatureMethodValue},
            {timestamp, timestampValue},
            {token, tokenValue},
            {version, versionValue},
            {trackParam, trackValue},
        };

        foreach (var pair in oauthKeyValuePairs)
        {
            parameterBuilder.Append(string.Format("{0}={1}&", pair.Key, pair.Value));
        }

        string parameters = parameterBuilder.ToString().Substring(0, parameterBuilder.Length - 1);

        StringBuilder signatureBaseString = new StringBuilder();
        signatureBaseString.Append(httpMethodValue.ToUpper());
        signatureBaseString.Append("&");
        signatureBaseString.Append(Uri.EscapeDataString(urlValue));
        signatureBaseString.Append("&");
        signatureBaseString.Append(Uri.EscapeDataString(parameters));

        return signatureBaseString.ToString();
    }

    public static string InitSignatureValue(string signatureBaseString) 
    {
        String signingKey = Uri.EscapeDataString(consumerSecretValue) + "&" + Uri.EscapeDataString(tokenSecretValue);

        HMACSHA1 hash = new HMACSHA1( new ASCIIEncoding().GetBytes(signingKey));

        String signature = Convert.ToBase64String(hash.ComputeHash(new ASCIIEncoding().GetBytes(signatureBaseString.ToString())));
        return signature;
    }

}