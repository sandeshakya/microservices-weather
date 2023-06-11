#!/bin/bash
set -e

aws ecr get-login-password --region us-east-1 --profile weather-ecr-agent | docker login --username AWS --password-stdin 921021806075.dkr.ecr.us-east-1.amazonaws.com
docker build -t cloud-weather-temperature .
docker tag cloud-weather-temperature:latest 921021806075.dkr.ecr.us-east-1.amazonaws.com/cloud-weather-temperature:latest
docker push 921021806075.dkr.ecr.us-east-1.amazonaws.com/cloud-weather-temperature:latest