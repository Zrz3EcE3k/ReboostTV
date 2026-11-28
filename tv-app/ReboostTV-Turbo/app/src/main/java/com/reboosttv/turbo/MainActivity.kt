package com.reboosttv.turbo 

import android.app.Activity
import android.app.ActivityManager
import android.content.Context
import android.content.Intent
import android.graphics.Color
import android.graphics.Typeface
import android.graphics.drawable.GradientDrawable
import android.graphics.drawable.StateListDrawable
import android.net.Uri
import android.os.Bundle
import android.provider.Settings
import android.view.Gravity
import android.widget.Button
import android.widget.LinearLayout
import android.widget.TextView
import android.widget.Toast
import androidx.core.content.ContextCompat

class MainActivity : Activity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val root = LinearLayout(this).apply {
            orientation = LinearLayout.VERTICAL
            gravity = Gravity.CENTER
            setBackgroundColor(Color.parseColor("#0F172A"))
            setPadding(80, 28, 80, 28)
        }

        val title = TextView(this).apply {
            text = "⚡ ReboostTV Turbo"
            textSize = 30f
            setTextColor(Color.WHITE)
            typeface = Typeface.DEFAULT_BOLD
            gravity = Gravity.CENTER
            setPadding(0, 0, 0, 36)
        }
        root.addView(title)

        val btnStartHud = createTvButton("ZAPNOUT MIZEJÍCÍ HUD (AUTO-HIDE)", "#10B981") {
            if (!Settings.canDrawOverlays(this)) {
                Toast.makeText(this, "Povolte zobrazování přes ostatní aplikace", Toast.LENGTH_LONG).show()
                startActivity(Intent(Settings.ACTION_MANAGE_OVERLAY_PERMISSION, Uri.parse("package:$packageName")))
            } else {
                ContextCompat.startForegroundService(this, Intent(this, HudService::class.java))
                Toast.makeText(this, "HUD zapnut (vlevo nahoře)", Toast.LENGTH_SHORT).show()
            }
        }

        val btnStopHud = createTvButton("VYPNOUT HUD", "#3B5998") {
            stopService(Intent(this, HudService::class.java))
            Toast.makeText(this, "HUD vypnut", Toast.LENGTH_SHORT).show()
        }

        val btnKillSwitch = createTvButton("🚀 KILL SWITCH (UVOLNIT RAM)", "#0EA5E9") {
            val am = getSystemService(Context.ACTIVITY_SERVICE) as ActivityManager
            val pm = packageManager
            val packages = pm.getInstalledApplications(0)
            var count = 0
            for (pkg in packages) {
                if (pkg.packageName != packageName) {
                    am.killBackgroundProcesses(pkg.packageName)
                    count++
                }
            }
            Toast.makeText(this, "RAM uvolněna! Vyčištěno procesů: $count", Toast.LENGTH_SHORT).show()
        }

        root.addView(btnStartHud)
        root.addView(btnStopHud)
        root.addView(btnKillSwitch)

        val hint = TextView(this).apply {
            text = "💡 TIP PRO RYCHLÉ UVOLNĚNÍ RAM:\n" +
                   "Přidejte si ReboostTV Turbo na hlavní plochu do Oblíbených (tlačítko + na ploše TV).\n" +
                   "Když se TV zpomalí, stačí appku otevřít, kliknout na Kill Switch a vrátit se tlačítkem Domů.\n" +
                   "HUD se po restartu TV spouští automaticky."
            textSize = 14f
            setTextColor(Color.parseColor("#94A3B8"))
            gravity = Gravity.CENTER
            setPadding(20, 36, 20, 0)
        }
        root.addView(hint)

        setContentView(root)
        btnStartHud.requestFocus()
    }

    private fun createTvButton(label: String, baseColorHex: String, onClick: () -> Unit): Button {
        val normalBg = GradientDrawable().apply {
            setColor(Color.parseColor(baseColorHex))
            cornerRadius = 16f
        }
        val focusedBg = GradientDrawable().apply {
            setColor(Color.parseColor(baseColorHex))
            cornerRadius = 16f
            setStroke(8, Color.parseColor("#FACC15")) // jasně žlutý rámeček při výběru ovladačem
        }

        val states = StateListDrawable().apply {
            addState(intArrayOf(android.R.attr.state_focused), focusedBg)
            addState(intArrayOf(), normalBg)
        }

        return Button(this).apply {
            text = label
            textSize = 18f
            setTextColor(Color.WHITE)
            typeface = Typeface.DEFAULT_BOLD
            background = states
            isFocusable = true
            isFocusableInTouchMode = true
            layoutParams = LinearLayout.LayoutParams(900, 90.apply {
                setMargins(0, 14, 0, 14)
            }
            setOnFocusChangeListener { v, hasFocus ->
                v.scaleX = if (hasFocus) 1.04f else 1.0f
                v.scaleY = if (hasFocus) 1.04f else 1.0f
            }
            setOnClickListener { onClick() }
        }
    }
}