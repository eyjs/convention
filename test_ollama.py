import http.client
import json

host = "localhost"
port = 11434
path = "/api/generate"

headers = {
    "Content-Type": "application/json"
}

data = {
    "model": "llama3",
    "prompt": "Why is the sky blue?",
    "stream": False
}

body = json.dumps(data)

conn = http.client.HTTPConnection(host, port)
conn.request("POST", path, body, headers)

response = conn.getresponse()

print(response.status, response.reason)
print(response.read().decode())

conn.close()