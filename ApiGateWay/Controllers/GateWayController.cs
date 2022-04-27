using Common;
using EasyCaching.Core;
using Gateway.Business;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateWay.Controllers
{
    [Route("/gateway")]
    [ApiController]
    public class GateWayController : ControllerBase
    {
        //private IEasyCachingProvider _easyCachingProvider;
        //private IEasyCachingProviderFactory _easyCachingProviderFactory;
        private readonly IGateWayService _gateWayService;
        public GateWayController(IGateWayService gateWayService)
        {
            //_easyCachingProviderFactory = easyCachingProviderFactory;
            //_easyCachingProvider = _easyCachingProviderFactory.GetCachingProvider("redis1");
            _gateWayService = gateWayService;

        }
        [HttpPost("/gateway/token")]
        public IActionResult GetToken(TokenRequest request)
        {
            var tokenResponse = new TokenResponse();
            if (string.IsNullOrEmpty(request.ApiKey) || string.IsNullOrEmpty(request.ApiPass))
            {
                tokenResponse.ErrorMessage = "api key and pass must not be empty";
                return BadRequest(tokenResponse);
            }
            tokenResponse = _gateWayService.GetToken(request);
            if (tokenResponse.IsOk)
            {
                return Ok(tokenResponse);
            }
            return BadRequest(tokenResponse);
        }
        [HttpPost("/gateway/makecryptoprocess")]
        public IActionResult MakeCryptoProcess(CryptographyRequest request)
        {
            var response = new CryptographyResponse();
            try
            {
                if (string.IsNullOrEmpty(request.Value))
                {
                    response.ErrorMessage = "Value must not be empty";
                    return BadRequest(response);
                }
                if (string.IsNullOrEmpty(request.Token))
                {
                    response.ErrorMessage = "Token must not be empty";
                    return BadRequest(response);
                }
                response = _gateWayService.SendCryptoProcess(request);
                if (response.IsOk)
                {
                    return Ok(response);
                }                
            }
            catch (Exception e)
            {
                response.ErrorMessage = e.Message;
            }
            return BadRequest(response);
        }
    }
}
