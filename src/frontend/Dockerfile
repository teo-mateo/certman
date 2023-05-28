FROM node:20

RUN npm i -g serve

RUN mkdir /certman
RUN mkdir /certman/app

COPY ./certman-ui/build /certman/app

RUN serve -s /certman/app -l 5051 &