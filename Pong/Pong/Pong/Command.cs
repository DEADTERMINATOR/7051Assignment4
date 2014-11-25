using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pong
{
    class Command
    {
        String text;
        public Boolean processed;

        public Command(String text)
        {
            this.text = text;
            processed = false;
        }

        public override String ToString()
        {
            return text;
        }
    }
}
