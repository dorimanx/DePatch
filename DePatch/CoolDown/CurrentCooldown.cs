using System;

namespace DePatch.CoolDown
{
    public class CurrentCooldown {

        private long _startTime;
        private readonly long _currentCooldown;

        private string command;

        public CurrentCooldown(long cooldown) {
            _currentCooldown = cooldown;
        }

        public void StartCooldown(string command) {
            this.command = command;
            _startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public long GetRemainingSeconds(string command) {

            if (this.command != command)
                return 0;

            long elapsedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _startTime;

            if (elapsedTime >= _currentCooldown) 
                return 0;

            return (_currentCooldown - elapsedTime) / 1000;
        }
    }
}
