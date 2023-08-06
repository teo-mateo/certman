# certman
Utility to generate Authority Certificates and signed SSL Certificates 

# to do

build and deployment instructions: 
1. run `build.sh prod`

    This will build the .net backend, the react js frontend and package it all in an image. 
The resulting image will be named "certman:<version>" where <version> is just a number. Make note of the new version number, and use it in the next step:

1. run `deploy.sh image:<version>`

    This will copy the image to the server
