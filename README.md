# sitecore.computer.vision

For details please refer to: https://miguelminoldo.fr/2021/02/10/sitecore-smart-cropping-azure-1/

# Installation steps:
1. Install the package with the installation wizard.
2. Configure the Azure Computer Vision services keys:
   a. <setting name="Sitecore.Computer.Vision.CroppingImageField.AICroppingField.CognitiveServices.ApiKey" value="{YOUR_APP_KEY}" />
   b. <setting name="Sitecore.Computer.Vision.CroppingImageField.AICroppingField.CognitiveServices.ApiUrl" value="https://{YOUR_AZURE_SERVICE_URL}.cognitiveservices.azure.com/vision/v2.0/" />
   c. <setting name="Sitecore.Computer.Vision.CroppingImageField.AICroppingField.CognitiveServices.ApiZone" value="{YOUR_ZONE}" />
3. You can now make use of the new field "AI Cropping Image Field". 
