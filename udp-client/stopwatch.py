import datetime

class StopWatch(object):

    def __init__(self):
        self.restart()

    def restart(self):
        self.startTime = datetime.datetime.now()

    def elapsed(self) -> datetime.timedelta: 
        return datetime.datetime.now() - self.startTime

    def elapsed_ms(self) -> int:
        return self.elapsed() / datetime.timedelta(microseconds=1000)

