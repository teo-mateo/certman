#!/usr/bin/env bash 

for i in {1..5}
do
  (while true; do curl -k https://127.0.0.1:5011/api/system/info; sleep 0.02; done) &
done