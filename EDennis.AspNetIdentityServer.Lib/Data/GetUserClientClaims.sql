create procedure dbo.GetUserClientClaims 
	@UserId varchar(255), 
	@ClientId varchar(255)
as
begin

	--get claims associated with user, through app role
	select ClaimType, ClaimValue
		from AspNetRoleClaims rc
		inner join TClientClaim cc
			on cc.ClaimType = rc.ClaimType
		inner join AspNetUserRoles ur
			on rc.RoleId = ur.RoleId
		where cc.ClientId = @ClientId
			and ur.UserId = @UserId
	--add claims associated with user, independent of app role
	union
	select ClaimType, ClaimValue
		from AspNetUserClaims uc
		inner join TClientClaim cc
			on cc.ClaimType = uc.ClaimType
		where cc.ClientId = @ClientId
			and uc.UserId = @UserId

END

