#!/usr/bin/python

import socket
import argparse
import stopwatch

parser = argparse.ArgumentParser(description="udp-py, small tool handling udp in python.\nProgram will send an UDP packet, then wait for N (default 1) response packets")
parser.add_argument('hostname', metavar='host', type=str, help='host to send to')
parser.add_argument('port', metavar='port', type=int, help='port number')
parser.add_argument('filename', metavar='filename', type=str, help='file to send')
parser.add_argument('--receive', dest='receive_count', metavar='count', type=int, help='How many packets to receive before terminate.', default=1)

args = parser.parse_args()


client_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
client_socket.settimeout(1.0)
message = b'test'
addr = (args.hostname, args.port)

msg = bytearray()

with open(args.filename, 'rb') as reader:
    msg = reader.read()

print(f'Sending {len(msg)} bytes to {args.hostname}:{args.port}')
client_socket.sendto(msg, addr)
print('sent')
watch = stopwatch.StopWatch()

try:
    # Create a UDP socket
    # Notice the use of SOCK_DGRAM for UDP packets
    serverSocket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    # Assign IP address and port number to socket
    serverSocket.bind(('192.168.6.13', 5067))


    for n in range(args.receive_count):
        data, server = serverSocket.recvfrom(4096)
        elapsed = watch.elapsed_ms()
        print(f'After {elapsed}ms recevied \n{data}')

    print('ending without errors')
except socket.timeout as e:
    print(f'REQUEST TIMED OUT after {watch.elapsed_ms()}: {e}')

