package com.eappraisal.commentsfeedback.service;

import com.eappraisal.commentsfeedback.entity.Comment;
import java.util.List;

public interface CommentService {
    List<Comment> getCommentsByAppraisalId(Long appraisalId);
    List<Comment> getCommentsByAppraisalIdAndVisibility(Long appraisalId, String visibility);
    Comment save(Comment comment);
}
