# OTP Feature

## Main Steps

1. Front End sends an HTTP POST request to the API to generate an OTP
   * Request Body should contain the user's mobile number.
     ```json
     {
       "mobileNumber": "string"
     }
     ```
     
2. API generates an OTP and sends it to the user's mobile number using SMS Gateway. Front End is notified that the OTP is sent. OTP is not sent through the HTTP Response. It is sent through SMS Gateway.
   * Response Body should contain the user's mobile number.
     ```json
     {
       "otpStatus": "sent"
     }
     ```
3. Front End sends an HTTP POST request to the API to verify the OTP
    * Request Body should contain the user's mobile number and the OTP user entered.
      ```json
      {
         "mobileNumber": "string",
         "otp": "string"
      }
      ```
4. API verifies the OTP and sends the result to the Front End.
    * Response Body should contain the user's mobile number and the OTP user entered.
      ```json
      {
         "isVerified": "boolean"
      }
      ```
      

