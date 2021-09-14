#!/usr/bin/python

import sys
import socket

import argparse

parser = argparse.ArgumentParser(description="udp-py, small tool handling udp in python.\nProgram will send an UDP packet, then wait for N (default 1) response packets")
parser.add_argument('hostname', metavar='host', type=str, help='host to send to')
parser.add_argument('port', metavar='port', type=int, help='port number')
parser.add_argument('file', metavar='filename', nargs='?', type=str, help='file to send, defaults to stdin', default='')
parser.add_argument('--receive', metavar='r', type=int, help='How many packets to receive before terminate.', default=1)

args = parser.parse_args()
