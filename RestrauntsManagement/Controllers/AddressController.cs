using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Filters;
using DotNetRestaurantManagement.Helpers;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace DotNetRestaurantManagement.Controllers
{
    [RoutePrefix("api/addresses")]
    public class AddressController : ApiController
    {
        private readonly IAddressService _addressService;
        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        /// Adds an address for the current user.
        [JwtAuthorize]
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> AddAddress([FromBody] AddressRequest request)
        {
            long userId = ClaimsHelper.GetUserId(User);
            var response = await _addressService.AddAddressAsync(userId, request);
            return Created("", new ApiResponse<object>(true, response, SuccessMessages.AddressAdded));
        }

        ///Updates the user address
        [JwtAuthorize]
        [HttpPost]
        [Route("{addressId:long}")]
        public async Task<IHttpActionResult> UpdateAddress(long addressId, [FromBody] AddressRequest request)
        {
            long userId = ClaimsHelper.GetUserId(User);
            var response = await _addressService.UpdateAddressAsync(userId, addressId, request);
            return Ok(new ApiResponse<object>(true, response, SuccessMessages.AddressUpdated));
        }

        /// Read particular address
        [JwtAuthorize]
        [HttpGet]
        [Route("{addressId:long}")]
        public async Task<IHttpActionResult> GetAddress(long addressId)
        {
            long userId = ClaimsHelper.GetUserId(User);
            var response = await _addressService.GetAddressAsync(userId, addressId);
            return Ok(new ApiResponse<object>(true, response, SuccessMessages.AddressFetched));
        }

        /// Read all address of user
        [JwtAuthorize]
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetAllUserAddresses()
        {
            long userId = ClaimsHelper.GetUserId(User);
            var response = await _addressService.GetAllUserAddressesAsync(userId);

            return Ok(new ApiResponse<List<AddressResponse>>(
                true,
                response,
                SuccessMessages.UserAddressFetched));
        }

        /// Delete user address
        [JwtAuthorize]
        [HttpPut]
        [Route("{addressId:long}")]
        public async Task<IHttpActionResult> RemoveUserAddress(long addressId)
        {
            long userId = ClaimsHelper.GetUserId(User);
            await _addressService.RemoveUserAddressAsync(userId, addressId);
            return Ok(new ApiResponse<object>(
                true,
                SuccessMessages.RemovedAddress));
        }
    }
}
