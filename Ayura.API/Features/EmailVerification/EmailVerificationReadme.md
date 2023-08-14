# Email Verification (EVC)

After user creates an account, they will have to verify their email address.

## Main Steps

1. Front End sends an HTTP Post request to generate email verification code for user. Request body will have the email
   address.
2. Back End generates a unique code and stores in the emailverification model alongside verification status.
3. Back End sends an email to the user's email address with the unique code.
4. Front End sends an HTTP Post request to verify the code, HTTP Header will include the JWT token and the unique code
   in the body.
5. Back End verifies the code and updates the verification status in the user model.
6. Back End sends the verification status to the Front End.

### Storing and Cleaning Email Verification Code

1. One user can have only one verification code at a time. If the user requests an VC again, the previous code should be
   removed from the Database.
2. VC should be stored in the Database for a limited time. When the Timer expires, the VC should be removed from the
   Database.

## VerifyEmailService

1. GenerateVerificationCode
    * Generate a random Verification Code with 6 characters.
2. SendVerificationCode
    * Send the Verification Code to the user's email address using Email Gateway.
3. VerifyEmail
    * Verify the Verification Code entered by the user by searching for the email address & the Verification Code from
      the Database.



