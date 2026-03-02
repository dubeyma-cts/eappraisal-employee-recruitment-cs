package com.eappraisal.commentsfeedback.service.impl;

import com.eappraisal.commentsfeedback.dao.CommentRepository;
import com.eappraisal.commentsfeedback.entity.Comment;
import com.eappraisal.commentsfeedback.service.CommentService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
public class CommentServiceImpl implements CommentService {
    private final CommentRepository commentRepository;

    @Autowired
    public CommentServiceImpl(CommentRepository commentRepository) {
        this.commentRepository = commentRepository;
    }

    @Override
    public List<Comment> getCommentsByAppraisalId(Long appraisalId) {
        return commentRepository.findByAppraisalIdOrderByCreatedAtDesc(appraisalId);
    }

    @Override
    public List<Comment> getCommentsByAppraisalIdAndVisibility(Long appraisalId, String visibility) {
        return commentRepository.findByAppraisalIdAndVisibilityOrderByCreatedAtDesc(appraisalId, visibility);
    }

    @Override
    public Comment save(Comment comment) {
        // Business logic: set createdAt and validate comment length
        if (comment.getCreatedAt() == null) {
            comment.setCreatedAt(java.time.LocalDateTime.now());
        }
        if (comment.getBody() == null || comment.getBody().trim().length() < 3) {
            throw new IllegalArgumentException("Comment text must be at least 3 characters long");
        }
        if (comment.getAuthorUserId() == null || comment.getAuthorUserId() <= 0) {
            throw new IllegalArgumentException("Author ID is required");
        }
        if (comment.getVisibility() == null || comment.getVisibility().isBlank()) {
            throw new IllegalArgumentException("Visibility is required");
        }
        return commentRepository.save(comment);
    }
}
