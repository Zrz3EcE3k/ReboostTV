package com.reboosttv.turbo

import android.app.Activity
import android.app.ActivityManager
import android.content.Intent
import android.graphics.Color
import android.os.Bundle
import android.provider.Settings
import android.view.Gravity
import android.widget.Button
import android.widget.LinearLayout
import android.widget.TextView
import android.widget.Toast

class MainActivity : Activity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val root = LinearLayout(this).apply {
            orientation = LinearLayout.VERTICAL
            gravity = Gravity.CENTER
            setBackgroundColor(Color.parseColor("#12141A"))
            setPadding(64, 64, 64, 64)
        }

        val title = TextView(this).apply {
            text = "⚡ ReboostTV Turbo"
            textSize = 28f
            setTextColor(Color.WHITE)
            gravity = Gravity.CENTER
        }

        val btnStartHud = makeButton("Zapnout mizející HUD (Auto-Hide)", "#10B981") {
            if (!Settings.canDrawOverlays(this)) {
                Toast.makeText(this, "Povolte vykreslování přes aplikace (nebo přes ReboostTV.exe)", Toast.LENGTH_LONG).show()
                return@makeButton
            }
            val i = Intent(this, HudService::class.java).putExtra("autoHide", true)
            startForegroundService(i)
            Toast.makeText(this, "HUD zapnut (Auto-Hide aktivní)", Toast.LENGTH_SHORT).show()
        }

        val btnStopHud = makeButton("Vypnout HUD", "#374151") {
            stopService(Intent(this, HudService::class.java))
            Toast.makeText(this, "HUD vypnut", Toast.LENGTH_SHORT).show()
        }

        val btnKillRam = makeButton("🚀 Kill Switch (Uvolnit RAM)", "#0084FF") {
            val am = getSystemService(ACTIVITY_SERVICE) as ActivityManager
            am.runningAppProcesses?.forEach { proc ->
                if (proc.processName != packageName) {
                    am.killBackgroundProcesses(proc.processName)
                }
            }
            Toast.makeText(this, "RAM uvolněna!", Toast.LENGTH_SHORT).show()
        }

        root.addView(title)
        root.addView(btnStartHud)
        root.addView(btnStopHud)
        root.addView(btnKillRam)
        setContentView(root)
    }

    private fun makeButton(label: String, colorHex: String, onClick: () -> Unit): Button {
        return Button(this).apply {
            text = label
            setBackgroundColor(Color.parseColor(colorHex))
            setTextColor(Color.WHITE)
            textSize = 18f
            isFocusable = true
            layoutParams = LinearLayout.LayoutParams(620, 110).apply {
                topMargin = 28
            }
            setOnClickListener { onClick() }
        }
    }
}