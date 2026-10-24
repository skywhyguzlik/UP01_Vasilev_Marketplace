using System;

namespace MarketplaceApp.Classes
{
    
    public static class CaptchaHelper
    {
        private static readonly Random _random = new Random();

        
        public static string Generate(int length = 6)
        {
            String allowchar = "A,B,C,D,E,F,G,H,J,K,L,M,N,P,Q,R,S,T,U,V,W,X,Y,Z,";
            allowchar += "a,b,c,d,e,f,g,h,i,j,k,m,n,p,q,r,s,t,u,v,w,x,y,z,";
            allowchar += "2,3,4,5,6,7,8,9";
            char[] separator = { ',' };
            String[] ar = allowchar.Split(separator);

            String captcha = "";
            for (int i = 0; i < length; i++)
                captcha += ar[_random.Next(0, ar.Length)];
            return captcha;
        }

        
        public static bool Check(string input, string captcha)
        {
            return !string.IsNullOrEmpty(input) && input.Trim() == captcha;
        }
    }
}
