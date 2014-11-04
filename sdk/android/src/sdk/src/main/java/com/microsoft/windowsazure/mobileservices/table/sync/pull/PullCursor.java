package com.microsoft.windowsazure.mobileservices.table.sync.pull;

import com.microsoft.windowsazure.mobileservices.table.query.Query;

/**
 * Created by marianosanchez on 11/3/14.
 * An object to represent the current position of pull in full query resullt
 */
public class PullCursor {

    int totalRead; // used to track how many we have read so far since the last delta
    int initialSkip;
    int remaining;

    public PullCursor(Query query) {

        this.remaining = query.getTop();

        this.initialSkip = query.getSkip();
    }

    public int getPosition() {
        return this.initialSkip + this.totalRead;
    }

    public void setRemaining(int remaining) {
        this.remaining = remaining;
    }

    public int getRemaining() {
        return this.remaining;
    }

    public boolean getComplete() {
        return this.remaining <= 0;
    }

    /**
     * Called when ever an item is processed from result
     * @return True if cursor is still open, False when it is completed.
     */
    public boolean onNext() {
        if (this.getComplete()) {
            return false;
        }

        this.remaining--;
        this.totalRead++;

        return true;
    }

    /**
     * Called when delta token is modified
     */
    public void reset() {
        this.initialSkip = 0;
        this.totalRead = 0;
    }
}
