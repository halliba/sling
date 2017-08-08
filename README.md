# sling
Welcome to sling, a command line utility to easily send files through your local network.

sling uses an UDP Broadcast to advertise files but the reliable TCP protocoll to send them.

#### Usage

1. Download and extract a release.
   (Optional: Add the extracted folder to your environment path variable to use sling wherever you are.)
2. On the machine holding the file:

   `sling [filename]`
   
   On the target machines:
   
   `sling -r`
