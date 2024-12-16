# [Enterspeed Struct Source](https://github.com/enterspeedhq/enterspeed-source-struct-pim)

Enterspeed Struct Source is an Azure function that takes content from Struct and ingests into Enterspeed. 
The Azure function exposes an url that can be used when setting up a webhook in Struct.

## Test locally

To test the function locally you must do the following:

1) Setup configuration
   1) Copy the `example_local.settings.json` file and rename the copy to `local.settings.json`
   2) Fil out the empty settings like API keys and urls for Enterspeed and Struct
2) Start the Azure function in Visual Studio or what ever editor you are using 
3) Expose the local url of the Azure function to the Internet
   1) Create a free account on https://ngrok.com/
   2) Download ngrok and setup ngrok https://ngrok.com/download
   3) Start a tunnel on the port where the Azure function is running with the following command promt: `ngrok http 7071`
4) Setup a webhook in Struct
   1) Setup a webhook in Struct using the ngrok url
5) Publish a product or asset from Struct to push it to Enterspeed

### Seeding

You can seed data by making a GET request to one of the following endpoints:

- `/api/SeedAll`
- `/api/SeedGlobalLists`
- `/api/SeedAssets`
- `/api/SeedCategories`
- `/api/SeedVariants`
- `/api/SeedProducts`