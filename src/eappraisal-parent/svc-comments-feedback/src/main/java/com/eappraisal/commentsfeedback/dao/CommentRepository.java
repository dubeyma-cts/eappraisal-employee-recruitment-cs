package com.eappraisal.commentsfeedback.dao;

import com.eappraisal.commentsfeedback.entity.Comment;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;

@Repository
public interface CommentRepository extends JpaRepository<Comment, Long> {
    List<Comment> findByAppraisalIdOrderByCreatedAtDesc(Long appraisalId);
    List<Comment> findByAppraisalIdAndVisibilityOrderByCreatedAtDesc(Long appraisalId, String visibility);
}
