package com.eappraisal.commentsfeedback.controller;

import com.eappraisal.commentsfeedback.entity.Comment;
import com.eappraisal.commentsfeedback.service.CommentService;
import com.eappraisal.commentsfeedback.dto.CommentRequestDTO;
import com.eappraisal.commentsfeedback.dto.CommentResponseDTO;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import jakarta.validation.Valid;

import java.util.List;

@RestController
@CrossOrigin(origins = "*")
@RequestMapping("/api/v1/feedback/comments")
public class CommentController {
    private final CommentService commentService;

    @Autowired
    public CommentController(CommentService commentService) {
        this.commentService = commentService;
    }


    @GetMapping("/appraisal/{appraisalId}")
    public ResponseEntity<List<CommentResponseDTO>> getCommentsByAppraisal(@PathVariable Long appraisalId) {
        List<Comment> comments = commentService.getCommentsByAppraisalId(appraisalId);
        List<CommentResponseDTO> dtos = comments.stream().map(this::mapToResponse).toList();
        return ResponseEntity.ok(dtos);
    }

    @GetMapping("/appraisal/{appraisalId}/manager")
    public ResponseEntity<List<CommentResponseDTO>> getManagerCommentsByAppraisal(@PathVariable Long appraisalId) {
        List<Comment> comments = commentService.getCommentsByAppraisalIdAndVisibility(appraisalId, "Mgr");
        List<CommentResponseDTO> dtos = comments.stream().map(this::mapToResponse).toList();
        return ResponseEntity.ok(dtos);
    }

    @PostMapping
    public ResponseEntity<CommentResponseDTO> createComment(@Valid @RequestBody CommentRequestDTO request) {
        String visibility = request.getVisibility() == null || request.getVisibility().isBlank()
                ? "All"
                : request.getVisibility().trim();
        return ResponseEntity.ok(saveComment(request, visibility));
    }

    @PostMapping("/manager")
    public ResponseEntity<CommentResponseDTO> createManagerComment(@Valid @RequestBody CommentRequestDTO request) {
        return ResponseEntity.ok(saveComment(request, "Mgr"));
    }

    @PostMapping("/employee-feedback")
    public ResponseEntity<CommentResponseDTO> createEmployeeFeedback(@Valid @RequestBody CommentRequestDTO request) {
        return ResponseEntity.ok(saveComment(request, "All"));
    }

    private CommentResponseDTO saveComment(CommentRequestDTO request, String visibility) {
        // Map DTO to entity
        Comment comment = new Comment();
        comment.setAppraisalId(request.getAppraisalId());
        comment.setBody(request.getCommentText());
        comment.setAuthorUserId(request.getAuthorId());
        comment.setVisibility(visibility);
        comment.setCreatedAt(java.time.LocalDateTime.now());
        Comment saved = commentService.save(comment);
        return mapToResponse(saved);
    }

    private CommentResponseDTO mapToResponse(Comment comment) {
        CommentResponseDTO dto = new CommentResponseDTO();
        dto.setId(comment.getCommentId());
        dto.setAppraisalId(comment.getAppraisalId());
        dto.setCommentText(comment.getBody());
        dto.setAuthorId(comment.getAuthorUserId());
        dto.setVisibility(comment.getVisibility());
        dto.setCreatedAt(comment.getCreatedAt() != null ? comment.getCreatedAt().toString() : null);
        return dto;
    }
}
