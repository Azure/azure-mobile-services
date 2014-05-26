package com.microsoft.windowsazure.mobileservices;

import android.os.AsyncTask;
import android.os.Build;

import java.util.concurrent.BlockingQueue;
import java.util.concurrent.Executor;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.ThreadPoolExecutor;
import java.util.concurrent.TimeUnit;

/**
 * Created by absurd on 14-5-14.
 */
public class AsyncTaskUtil {

    private static final int corePoolSize = 15;
    private static final int maximumPoolSize = 30;
    private static final int keepAliveTime = 5;

    private static final BlockingQueue<Runnable> workQueue = new LinkedBlockingQueue<Runnable>(maximumPoolSize);
    private static final Executor threadPoolExecutor = new ThreadPoolExecutor(
            corePoolSize, maximumPoolSize, keepAliveTime, TimeUnit.SECONDS, workQueue);

    public static void addTaskInPool(AsyncTask asyncTask) {
        if(Build.VERSION.SDK_INT >= Build.VERSION_CODES.HONEYCOMB){
            asyncTask.executeOnExecutor(threadPoolExecutor,null);
        }else{
            asyncTask.execute();
        }
    }
}
