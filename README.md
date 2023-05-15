# mongo-db-dotnet-csfle
Demo project on Mongo DB client side field level encryption with dot net 6 web api, running in a docker container

# References
https://www.mongodb.com/docs/manual/core/csfle/
https://github.com/mongodb-university/docs-in-use-encryption-examples/tree/main/csfle/dotnet/local/reader/CSFLE

# Pre-requisite
1. Dotnet 6 SDK
2. Docker
3. Mongo DB enterprise running as a container in docker
https://www.mongodb.com/docs/manual/tutorial/install-mongodb-enterprise-with-docker/
Pull mongodb image
docker pull mongodb/mongodb-enterprise-server:latest
Run the image as a container
docker run --name mongodb -p 27017:27017 -d mongodb/mongodb-enterprise-server:latest
4. Install MongoDB Compass for querying, optimizing, and analyzing your MongoDB data
https://www.mongodb.com/products/compass

# Setup Mongo DB
Open MongoDB environment using MongoDB Compass and create the following databases and collections
1. Database = BookStore, Collection = Books
2. Database = encryption, Collection = __keyVault
3. Database = medicalRecords, Collection = patients

# Set up the Data Encryption key
Run the application
Open the Swagger endpoint
Execute the controller actin CSFLESetup/Post
