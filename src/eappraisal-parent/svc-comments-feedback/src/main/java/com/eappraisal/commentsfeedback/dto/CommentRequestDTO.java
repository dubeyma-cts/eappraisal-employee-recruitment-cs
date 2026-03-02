package com.eappraisal.commentsfeedback.dto;

import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;

public class CommentRequestDTO {
    @NotNull(message = "Appraisal ID is required")
    private Long appraisalId;

    @NotBlank(message = "Comment text is required")
    @jakarta.validation.constraints.Size(min = 3, max = 2000, message = "Comment text must be between 3 and 2000 characters")
    private String commentText;

    @NotNull(message = "Author ID is required")
    private Long authorId;

    private String visibility;

    // Getters and setters
    public Long getAppraisalId() { return appraisalId; }
    public void setAppraisalId(Long appraisalId) { this.appraisalId = appraisalId; }
    public String getCommentText() { return commentText; }
    public void setCommentText(String commentText) { this.commentText = commentText; }
    public Long getAuthorId() { return authorId; }
    public void setAuthorId(Long authorId) { this.authorId = authorId; }
    public String getVisibility() { return visibility; }
    public void setVisibility(String visibility) { this.visibility = visibility; }
}
