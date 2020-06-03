﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using AutoMapper;
using Forum.Api.Requests;
using Forum.Api.Responses;
using Forum.Core.Abstract.Managers;
using Forum.Core.Concrete.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Api.Controllers.v1
{
    [Authorize]
    [ApiController]
    [Route("/api/v1/posts/")]
    public class RepliesController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IReplyManager _replyManager;
        private readonly IPostManager _postManager;
        private readonly IMapper _mapper;

        public RepliesController(IReplyManager replyManager, IPostManager postManager, IMapper mapper,
            UserManager<User> userManager)
        {
            _userManager = userManager;
            _replyManager = replyManager;
            _postManager = postManager;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet("{postId:min(1)}/replies")]
        public async Task<IActionResult> GetAllReplies([FromRoute] int postId)
        {
            var post = await _postManager.GetPostWithReplies(postId);
            if (post == null)
            {
                return NotFound("Post does not exist");
            }

            var response = _mapper.Map<IEnumerable<ReplyResponse>>(post.Replies);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet("{postId:min(1)}/replies/{replyId:min(1)}")]
        public async Task<IActionResult> GetReply([FromRoute] int postId, [FromRoute] int replyId)
        {
            if (!await _postManager.PostExists(postId))
            {
                return NotFound("Post does not exist");
            }

            var reply = await _replyManager.GetReply(replyId);
            if (reply == null)
            {
                return NotFound("Reply does not exist");
            }

            var response = _mapper.Map<ReplyResponse>(reply);

            return Ok(response);
        }

        [HttpPut("{postId:min(1)}/replies/{id:min(1)}")]
        public async Task<IActionResult> EditReply([FromRoute] int postId, [FromRoute] int id,
            [FromBody] ReplyRequest request)
        {
            if (postId != request.PostId)
            {
                return BadRequest("Post id in route doesn't match request post id");
            }

            if (!await _postManager.PostExists(postId))
            {
                return NotFound("Post does not exist");
            }

            var reply = await _replyManager.GetReply(id);
            if (reply == null)
            {
                return NotFound("Reply does not exist");
            }

            var currentUserId = _userManager.GetUserId(HttpContext.User);
            if (reply.AuthorId != currentUserId)
            {
                return Forbid("You are not author of reply");
            }

            _mapper.Map(request, reply);
            reply.DateEdited = DateTime.Now;
            await _replyManager.SaveChangesAsync();

            var response = _mapper.Map<ReplyResponse>(reply);
            return Ok(response);
        }

        [HttpDelete("{postId:min(1)}/replies/{replyId:min(1)}")]
        public async Task<IActionResult> DeleteReply([FromRoute] int postId, [FromRoute] int replyId)
        {
            var reply = await _replyManager.GetReply(replyId);
            if (reply == null)
            {
                return NotFound("Reply not found");
            }

            if (reply.PostId != postId)
            {
                return BadRequest("Post id in route doesn't match request post id");
            }

            var currentUserId = _userManager.GetUserId(HttpContext.User);
            if (reply.AuthorId != currentUserId)
            {
                return Forbid("You are not author of reply");
            }

            _replyManager.RemoveReply(reply);
            await _replyManager.SaveChangesAsync();

            return Ok();
        }
    }
}