# OTP Feature

## Main Steps

1. Front End sends an HTTP POST request to the API to generate an OTP
    * Request Body should contain the user's mobile number.
      ```json
      {
        "mobileNumber": "string"
      }
      ```
2. API generates an OTP and sends it to the user's mobile number using SMS Gateway. Front End is notified that the OTP
   is sent. OTP is not sent through the HTTP Response. It is sent through SMS Gateway.
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
    * Response Body should contain the verification status.
      ```json
      {
         "isVerified": "boolean"
      }
      ```

## OTP Management

### Storing and Cleaning OTPs

1. One user can have only one OTP at a time. This can be implemented using a Dictionary with the user's mobile number as
   the key and the OTP as the value.
2. OTPs should be stored in the Dictionary for a limited time. This can be implemented using a Timer. When the Timer
   expires, the OTP should be removed from the Dictionary.

## OtpCreationService

1. GenerateOTP
    * Generate a random OTP.
2. SendOTP
    * Send the OTP to the user's mobile number using SMS Gateway.
2. VerifyOTP
    * Verify the OTP entered by the user by searching for the mobileNumber & the OTP from the Dictionary.

## OtpManagerService

1. Store OTP
    * Store the OTP in the Database.
2. Remove OTP
    * Remove the OTP from the Database.
    * This method should be called when the Timer expires.
3. Clean OTPs
    * Clean the OTPs from the Database.
4. Search OTPs
    * Search for the OTP in the Database.

## Storing in the Database

1. An OtpModel class should be created to store the OTPs in the Database.
2. The OtpModel class should contain the following properties.
    * MobileNumber
    * OTP
    * ExpiryTime
3. The ExpiryTime should be set to the current time + 5 minutes when the OTP is stored in the Database.


