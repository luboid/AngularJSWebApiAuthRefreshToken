AngularJS App with WebApi backend OAuth Bearer Token + Refresh Token 
===============================

**Project is using Microsoft MS SQL LocalDB 2014**

**You need to configure mail server into web.config**

    <mailSettings>
      <smtp from="AngularJSAuthRefreshToken <test@test.com>">
        <network host="mail.test.com" userName="test@test.com" password="test1234" port="25" />
      </smtp>
    </mailSettings>
  </system.net>

**Enter you public profile providers into database table [dbo].[ExternalLoginProviders].**

**Role authorization on then client side**

**To test refresh token set short live time for access_token into Startup.Auth.cs.**

    OAuthOptions = new OAuthAuthorizationServerOptions
    {
      ...
      AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(1),
      ...
    };

**Refresh token live time is configured into [dbo].[AspNetRefreshTokenApps].**

Test user: test@test.com, password: P@ssword123

Some ideas are borrowed from [Enable OAuth Refresh Tokens in AngularJS App using ASP .NET Web API 2, and Owin](http://bitoftech.net/2014/07/16/enable-oauth-refresh-tokens-angularjs-app-using-asp-net-web-api-2-owin/)