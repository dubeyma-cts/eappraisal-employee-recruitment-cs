package com.eappraisal.commentsfeedback.dto;

public class CommentResponseDTO {
    private Long id;
    private Long appraisalId;
    private String commentText;
    private Long authorId;
    private String visibility;
    private String createdAt;
    // Getters and setters
    public Long getId() { return id; }
    public void setId(Long id) { this.id = id; }
    public Long getAppraisalId() { return appraisalId; }
    public void setAppraisalId(Long appraisalId) { this.appraisalId = appraisalId; }
    public String getCommentText() { return commentText; }
    public void setCommentText(String commentText) { this.commentText = commentText; }
    public Long getAuthorId() { return authorId; }
    public void setAuthorId(Long authorId) { this.authorId = authorId; }
    public String getVisibility() { return visibility; }
    public void setVisibility(String visibility) { this.visibility = visibility; }
    public String getCreatedAt() { return createdAt; }
    public void setCreatedAt(String createdAt) { this.createdAt = createdAt; }
}
