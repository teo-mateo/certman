import http from 'k6/http';
import { sleep } from 'k6';

export const options = {
    vus: 1000,
    duration: '5m',
  };

export default function () {
  http.get('http://172.22.160.1:5010/api/certs/ca-certs');
  sleep(1);
}
