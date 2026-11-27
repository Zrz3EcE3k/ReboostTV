package com.reboosttv.turbo

import android.app.ActivityManager
import android.app.Notification
import android.app.NotificationChannel
import android.app.NotificationManager
import android.app.Service
import android.app.usage.UsageStatsManager
import android.content.Context
import android.content.Intent
import android.graphics.Color
import android.graphics.PixelFormat
import android.graphics.drawable.GradientDrawable
import android.os.Handler
import android.os.IBinder
import android.os.Looper
import android.provider.Settings
import android.view.Gravity
import android.view.View
import android.view.WindowManager
import android.widget.TextView

class HudService : Service() {

    private lateinit var windowManager: WindowManager
    private lateinit var hudView: TextView
    private val handler = Handler(Looper.getMainLooper())
    private var autoHide = true

    private val updateTask = object : Runnable {
        override fun run() {
            updateHud()
            handler.postDelayed(this, 2500)
        }
    }

    override fun onStartCommand(intent: Intent?, flags: Int, startId: Int): Int {
        autoHide = intent?.getBooleanExtra("autoHide", true) ?: true
        return START_STICKY
    }

    override fun onCreate() {
        super.onCreate()
        startForegroundNotification()

        windowManager = getSystemService(WINDOW_SERVICE) as WindowManager
        hudView = TextView(this).apply {
            setTextColor(Color.parseColor("#E2E8F0"))
            textSize = 13f
            setPadding(24, 10, 24, 10)
            background = GradientDrawable().apply {
                setColor(Color.parseColor("#CC12141A"))
                cornerRadius = 40f
                setStroke(2, Color.parseColor("#10B981"))
            }
            text = "⚡ ReboostTV"
        }

        val params = WindowManager.LayoutParams(
            WindowManager.LayoutParams.WRAP_CONTENT,
            WindowManager.LayoutParams.WRAP_CONTENT,
            WindowManager.LayoutParams.TYPE_APPLICATION_OVERLAY,
            WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE or WindowManager.LayoutParams.FLAG_NOT_TOUCHABLE,
            PixelFormat.TRANSLUCENT
        ).apply {
            gravity = Gravity.TOP or Gravity.END
            x = 28
            y = 24
        }

        windowManager.addView(hudView, params)
        handler.post(updateTask)
    }

    private fun updateHud() {
        if (autoHide && !isOnHomeScreen()) {
            hudView.visibility = View.GONE
            return
        }
        hudView.visibility = View.VISIBLE

        val am = getSystemService(ACTIVITY_SERVICE) as ActivityManager
        val memInfo = ActivityManager.MemoryInfo()
        am.getMemoryInfo(memInfo)
        val availMb = memInfo.availMem / (1024 * 1024)
        val totalMb = memInfo.totalMem / (1024 * 1024)

        val dnsMode = Settings.Global.getString(contentResolver, "private_dns_mode") ?: "off"
        val dnsIcon = if (dnsMode == "hostname") "🛡️ DNS ON" else "DNS OFF"

        hudView.text = "⚡ RAM: ${availMb}/${totalMb} MB  |  $dnsIcon"
    }

    private fun isOnHomeScreen(): Boolean {
        val usm = getSystemService(Context.USAGE_STATS_SERVICE) as UsageStatsManager
        val now = System.currentTimeMillis()
        val stats = usm.queryUsageStats(UsageStatsManager.INTERVAL_DAILY, now - 10_000, now)
        val topPkg = stats?.maxByOrNull { it.lastTimeUsed }?.packageName ?: return true

        val homeIntent = Intent(Intent.ACTION_MAIN).addCategory(Intent.CATEGORY_HOME)
        val homePkg = packageManager.resolveActivity(homeIntent, 0)?.activityInfo?.packageName
        return topPkg == homePkg || topPkg == packageName
    }

    private fun startForegroundNotification() {
        val channelId = "reboost_hud_channel"
        val nm = getSystemService(NotificationManager::class.java)
        nm.createNotificationChannel(
            NotificationChannel(channelId, "ReboostTV HUD", NotificationManager.IMPORTANCE_LOW)
        )
        val notif = Notification.Builder(this, channelId)
            .setContentTitle("ReboostTV Turbo HUD")
            .setContentText("Aktivní monitorování TV")
            .setSmallIcon(android.R.drawable.ic_menu_info_details)
            .build()
        startForeground(1, notif)
    }

    override fun onDestroy() {
        handler.removeCallbacks(updateTask)
        if (::hudView.isInitialized) windowManager.removeView(hudView)
        super.onDestroy()
    }

    override fun onBind(intent: Intent?): IBinder? = null
}