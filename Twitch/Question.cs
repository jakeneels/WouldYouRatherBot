using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twitch
{
    class Question
    {
        double gPercent1, gPercent2;
        int selected1 = 0; 
        int selected2 = 0;
        int index;
        int voteScore = 0;

        public void setIndex(int index)
        {
            this.index = index;
        }
        public int getVoteScore()
        {
            return voteScore;
        }
        public void incVoteScore()
        {
            voteScore++;
        }
        public void decVoteScore()
        {
            voteScore--;
        }
        public int getIndex()
        {
            return index;
        }
        public int getSelected1()
        {
            return selected1;
        }
        public int getSelected2()
        {
            return selected2;
        }
        public void incSelected1()
        {
            selected1++;
        }
        public void incSelected2()
        {
            selected2++;
        }
        public void resetSelected()
        {
            selected1 = 0;
            selected2 = 0;
        }
    }
}
