import { NextAuthOptions } from "next-auth"
import DuendeIdentityServer6 from "next-auth/providers/duende-identity-server6";

export const authOptions: NextAuthOptions = {
  providers: [ 
    DuendeIdentityServer6({
      id: "id-server",
      clientId: "nextApp",
      clientSecret: "secret",
      issuer: process.env.ID_URL,
      authorization: {
        params: {
          scope: "openid profile auctionApp"
        }
      },
      idToken: true //the user info will be extracted from the id_token claims (instead of making a requset to the userinfo endpoint)
    })
  ],
  session: {
    strategy: "jwt" //where to store user session - encrypted jwt in a session cookie
  },
  callbacks: {
    jwt: async ({token, profile, account}) => {
      //console.log({token, profile});
      if(profile?.username) token.username = profile?.username;
      //account will be populated only when the user first logs in
      if(account?.access_token) token.access_token = account.access_token;
      return token;
    },
    session: async ({session, token}) => {
      if(token) {
        session.user.username = token.username;
      }
      return session;
    }
  }
};