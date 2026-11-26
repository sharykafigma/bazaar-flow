#!/usr/bin/env python3
"""
Bazaar Flow audio generator.

Every sound in Assets/BazaarFlow/Audio is synthesized by this script from plain
math (sine waves, noise, Karplus-Strong plucked strings). No samples or
third-party recordings are used, so the output is our own work (see LICENSES.md).

Run:  python3 Tools/Audio/generate_audio.py
Needs: numpy. ffmpeg (with libvorbis) for the .ogg music loop.
"""
import os
import subprocess
import wave

import numpy as np

SR = 44100
ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT = os.path.join(ROOT, "Assets", "BazaarFlow", "Audio", "Resources", "BazaarFlowAudio")
rng = np.random.default_rng(7)  # fixed seed: same files every run


# ---------------------------------------------------------------- helpers

def t_axis(dur):
    return np.arange(int(SR * dur)) / SR


def env_ad(n, attack, decay_tau):
    """Fast attack, exponential decay. attack/decay in seconds."""
    t = np.arange(n) / SR
    a = np.clip(t / max(attack, 1e-4), 0, 1)
    return a * np.exp(-t / decay_tau)


def fade_out(x, dur=0.01):
    n = min(len(x), int(SR * dur))
    x[-n:] *= np.linspace(1, 0, n)
    return x


def sine(freq, dur, phase=0.0):
    t = t_axis(dur)
    if callable(freq):
        f = freq(t)
        return np.sin(2 * np.pi * np.cumsum(f) / SR + phase)
    return np.sin(2 * np.pi * freq * t + phase)


def lowpass(x, cutoff):
    """One-pole low-pass."""
    a = np.exp(-2 * np.pi * cutoff / SR)
    y = np.empty_like(x)
    acc = 0.0
    for i, v in enumerate(x):
        acc = (1 - a) * v + a * acc
        y[i] = acc
    return y


def highpass(x, cutoff):
    return x - lowpass(x, cutoff)


def pluck(freq, dur, bright=0.5, damp=0.996):
    """Karplus-Strong plucked string (santoor / rabab-like)."""
    n = int(SR * dur)
    p = max(2, int(SR / freq))
    buf = rng.uniform(-1, 1, p)
    buf = lowpass(buf, 1500 + 6000 * bright)
    out = np.zeros(n)
    idx = 0
    for i in range(n):
        v = buf[idx]
        out[i] = v
        nxt = buf[(idx + 1) % p]
        buf[idx] = damp * 0.5 * (v + nxt)
        idx = (idx + 1) % p
    out -= out.mean()
    return fade_out(out / (np.abs(out).max() + 1e-9), 0.02)


def drum(f_start, f_end, dur, tau, noise=0.0, noise_tau=0.01):
    """Membrane: pitched sine with downward glide, plus optional slap noise."""
    t = t_axis(dur)
    f = f_end + (f_start - f_end) * np.exp(-t / 0.03)
    x = np.sin(2 * np.pi * np.cumsum(f) / SR) * np.exp(-t / tau)
    if noise:
        x += noise * rng.uniform(-1, 1, len(t)) * np.exp(-t / noise_tau)
    return fade_out(x)


def tabla_na(dur=0.35):
    """Bright ringing right-hand tabla stroke (harmonic partials of ~ Sa)."""
    t = t_axis(dur)
    f0 = 440.0
    x = np.zeros_like(t)
    for k, amp, tau in [(1, 1.0, 0.12), (2, 0.6, 0.09), (3, 0.4, 0.06), (4, 0.25, 0.05), (5, 0.15, 0.04)]:
        x += amp * np.sin(2 * np.pi * f0 * k * t) * np.exp(-t / tau)
    x += 0.3 * rng.uniform(-1, 1, len(t)) * np.exp(-t / 0.004)
    return fade_out(x / np.abs(x).max())


def tabla_ge(dur=0.45):
    """Bass left-hand stroke (bayan) with the typical upward pitch glide."""
    t = t_axis(dur)
    f = 85 + 30 * (1 - np.exp(-t / 0.08))
    x = np.sin(2 * np.pi * np.cumsum(f) / SR) * np.exp(-t / 0.18)
    x += 0.15 * rng.uniform(-1, 1, len(t)) * np.exp(-t / 0.006)
    return fade_out(x / np.abs(x).max())


def normalize(x, peak=0.89):
    m = np.abs(x).max()
    return x * (peak / m) if m > 0 else x


def mix_into(dst, src, start_sample, gain=1.0, wrap=False):
    n = len(src)
    if wrap:
        idx = (np.arange(n) + start_sample) % len(dst)
        np.add.at(dst, idx, src * gain)
    else:
        end = min(len(dst), start_sample + n)
        dst[start_sample:end] += src[: end - start_sample] * gain


def write_wav(name, x, sr=SR):
    path = os.path.join(OUT, name)
    data = np.clip(x, -1, 1)
    pcm = (data * 32767).astype("<i2")
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(sr)
        w.writeframes(pcm.tobytes())
    return path


# ---------------------------------------------------------------- SFX

def sfx_tap():
    # Wooden crate knock: short low thump + click.
    x = drum(320, 180, 0.12, 0.03, noise=0.5, noise_tau=0.004)
    x += 0.4 * drum(900, 700, 0.12, 0.015)
    return normalize(highpass(x, 60), 0.8)


def sfx_belt_load():
    # Soft conveyor "clack": filtered noise burst with a tiny metallic ring.
    t = t_axis(0.09)
    n = rng.uniform(-1, 1, len(t)) * np.exp(-t / 0.012)
    n = lowpass(highpass(n, 400), 3500)
    ring = 0.25 * np.sin(2 * np.pi * 1250 * t) * np.exp(-t / 0.02)
    return normalize(fade_out(n + ring), 0.5)


def sfx_pickup():
    # Cheerful coin-ish "ting" (two partials, a fifth apart). Pitch is raised in-game per combo.
    t = t_axis(0.35)
    x = (np.sin(2 * np.pi * 1318.5 * t) * np.exp(-t / 0.10)
         + 0.5 * np.sin(2 * np.pi * 1975.5 * t) * np.exp(-t / 0.07)
         + 0.2 * np.sin(2 * np.pi * 2637 * t) * np.exp(-t / 0.04))
    x *= np.clip(t / 0.002, 0, 1)
    return normalize(fade_out(x), 0.6)


def sfx_shop_done():
    # Shop order complete: quick rising santoor arpeggio Sa-Ga-Pa-Sa'.
    notes = [523.25, 659.25, 783.99, 1046.5]
    out = np.zeros(int(SR * 0.9))
    for i, f in enumerate(notes):
        mix_into(out, pluck(f, 0.6, bright=0.8, damp=0.997), int(i * 0.07 * SR), 0.8)
    return normalize(fade_out(out, 0.05), 0.75)


def sfx_win():
    # Celebration: plucked phrase in a bazaar mode + tabla flourish + shimmer.
    beat = 0.14
    phrase = [(0, 587.33), (1, 659.25), (2, 739.99), (3, 880.0), (4, 987.77), (5, 880.0), (6, 1174.66)]
    out = np.zeros(int(SR * 2.2))
    for step, f in phrase:
        mix_into(out, pluck(f, 0.9, bright=0.7, damp=0.998), int(step * beat * SR), 0.7)
    # final chord
    for f in (587.33, 739.99, 880.0, 1174.66):
        mix_into(out, pluck(f, 1.3, bright=0.6, damp=0.9985), int(6 * beat * SR), 0.35)
    for i, stroke in enumerate([tabla_na, tabla_na, tabla_ge, tabla_na, tabla_na, tabla_ge]):
        mix_into(out, stroke(), int(i * beat * SR), 0.35)
    t = t_axis(len(out) / SR)
    shimmer = 0.06 * rng.uniform(-1, 1, len(t))
    shimmer = highpass(shimmer, 5000) * np.exp(-np.maximum(0, t - 0.84) / 0.4) * (t > 0.84)
    out += shimmer
    return normalize(fade_out(out, 0.2), 0.85)


def sfx_jam():
    # Belt Jam: grinding conveyor stop + descending "oh no" two-note.
    dur = 1.2
    t = t_axis(dur)
    # motor winding down
    f = 120 * np.exp(-t / 0.5) + 30
    motor = np.sign(np.sin(2 * np.pi * np.cumsum(f) / SR)) * 0.25 * np.exp(-t / 0.5)
    motor = lowpass(motor, 900)
    grind = lowpass(rng.uniform(-1, 1, len(t)), 1200) * 0.4 * np.exp(-t / 0.25)
    thunk = drum(140, 60, dur, 0.15, noise=0.4, noise_tau=0.01)
    out = motor + grind + 0.9 * thunk
    for i, fr in enumerate([392.0, 311.13]):  # G -> Eb, sad fall
        tt = t_axis(0.5)
        bend = fr * (1 - 0.03 * tt / 0.5)
        tone = np.sin(2 * np.pi * np.cumsum(bend) / SR)
        tone += 0.3 * np.sin(2 * 2 * np.pi * np.cumsum(bend) / SR)
        tone *= env_ad(len(tt), 0.01, 0.25)
        mix_into(out, tone, int((0.25 + i * 0.28) * SR), 0.5)
    return normalize(fade_out(out, 0.1), 0.8)


def sfx_ui_click():
    t = t_axis(0.06)
    x = np.sin(2 * np.pi * 1800 * t) * np.exp(-t / 0.01) + 0.4 * rng.uniform(-1, 1, len(t)) * np.exp(-t / 0.003)
    return normalize(fade_out(x), 0.45)


def sfx_unlock():
    # Lock opening: metallic click-clack + small chime.
    out = np.zeros(int(SR * 0.5))
    mix_into(out, drum(2500, 1800, 0.05, 0.008, noise=0.6, noise_tau=0.003), 0)
    mix_into(out, drum(2000, 1500, 0.05, 0.008, noise=0.6, noise_tau=0.003), int(0.07 * SR))
    mix_into(out, pluck(1567.98, 0.4, bright=0.9), int(0.12 * SR), 0.5)
    return normalize(fade_out(out, 0.05), 0.6)


def sfx_ui_popup():
    # Panel opens: short upward airy swoosh + soft pluck.
    t = t_axis(0.25)
    n = rng.uniform(-1, 1, len(t))
    n = lowpass(highpass(n, 800), 5000) * np.sin(np.pi * np.clip(t / 0.18, 0, 1)) * 0.5
    out = n.copy()
    mix_into(out, pluck(783.99, 0.25, bright=0.7), int(0.08 * SR), 0.5)
    return normalize(fade_out(out, 0.03), 0.5)


def sfx_star():
    # Star earned on the win screen: bright bell.
    t = t_axis(0.6)
    x = (np.sin(2 * np.pi * 1760 * t) * np.exp(-t / 0.2)
         + 0.45 * np.sin(2 * np.pi * 2637 * t) * np.exp(-t / 0.12)
         + 0.25 * np.sin(2 * np.pi * 4186 * t) * np.exp(-t / 0.06))
    x *= np.clip(t / 0.002, 0, 1)
    return normalize(fade_out(x, 0.05), 0.6)


def sfx_coin():
    # Coin counted: two-step "ka-ching".
    out = np.zeros(int(SR * 0.3))
    for i, f in enumerate([1975.5, 2637.0]):
        t = t_axis(0.22)
        tone = np.sin(2 * np.pi * f * t) * np.exp(-t / 0.06)
        mix_into(out, tone, int(i * 0.06 * SR), 1.0)
    return normalize(fade_out(out, 0.02), 0.5)


def sfx_purchase():
    # Purchase done: coin cascade + bright santoor chord.
    out = np.zeros(int(SR * 1.2))
    coin = sfx_coin()
    for i in range(5):
        mix_into(out, coin, int(i * 0.06 * SR), 0.5)
    for f in (659.25, 830.61, 987.77, 1318.5):
        mix_into(out, pluck(f, 0.9, bright=0.8, damp=0.998), int(0.3 * SR), 0.5)
    return normalize(fade_out(out, 0.1), 0.75)


def sfx_locked():
    # Not allowed / locked: low double buzz.
    out = np.zeros(int(SR * 0.3))
    for i in range(2):
        t = t_axis(0.09)
        tone = np.sign(np.sin(2 * np.pi * 140 * t)) * np.exp(-t / 0.05)
        mix_into(out, lowpass(tone, 1200), int(i * 0.12 * SR), 1.0)
    return normalize(fade_out(out, 0.02), 0.5)


# ---------------------------------------------------------------- Music

def music_loop():
    """
    Light, cheerful bazaar loop. 96 BPM, 8 bars of 4/4 (Keherwa-style tabla theka),
    tanpura-like drone on Sa/Pa and a plucked santoor melody in Raag Khamaj flavour
    (major scale with flat Ni on the way down). Rendered with wrap-around so the
    end flows straight into the start (seamless loop).
    """
    bpm = 96
    beat = 60.0 / bpm
    bars = 8
    total = int(SR * beat * 4 * bars)
    out = np.zeros(total)

    sa = 220.0  # A3 as Sa
    ratio = {"S": 1.0, "r": 16 / 15, "R": 9 / 8, "G": 5 / 4, "m": 4 / 3, "P": 3 / 2,
             "D": 5 / 3, "n": 16 / 9, "N": 15 / 8, "S'": 2.0, "R'": 9 / 4, "G'": 5 / 2}

    # --- Drone (tanpura-ish): Pa, Sa', Sa', Sa (low), each string re-plucked every bar.
    drone = np.zeros(total)
    order = [sa * 3 / 4, sa, sa, sa / 2]
    for b in range(bars):
        for i, f in enumerate(order):
            start = int((b * 4 + i) * beat * SR)
            s = pluck(f, beat * 4.5, bright=0.25, damp=0.9993)
            mix_into(drone, s, start, 1.0, wrap=True)
    drone = lowpass(drone, 2200)
    out += 0.16 * normalize(drone, 1.0)

    # --- Tabla, Keherwa: Dha Ge Na Ti | Na Ka Dhi Na  (8 eighth-notes per bar)
    na = tabla_na()
    ge = tabla_ge()
    ti = drum(900, 800, 0.12, 0.02, noise=0.4, noise_tau=0.004)
    ka = drum(200, 150, 0.08, 0.02, noise=0.5, noise_tau=0.004)
    dha = normalize(np.pad(na, (0, len(ge) - len(na))) + 0.9 * ge, 1.0)
    theka = [dha, ge, na, ti, na, ka, dha, na]
    gains = [0.95, 0.6, 0.55, 0.4, 0.55, 0.45, 0.85, 0.5]
    perc = np.zeros(total)
    for b in range(bars):
        for i, (st, g) in enumerate(zip(theka, gains)):
            jitter = int(rng.normal(0, 0.003) * SR)  # tiny human feel
            start = int((b * 4 + i * 0.5) * beat * SR) + jitter
            mix_into(perc, st, max(0, start), g, wrap=True)
        # small fill on the last bar
        if b == bars - 1:
            for k in range(4):
                start = int((b * 4 + 3 + k * 0.25) * beat * SR)
                mix_into(perc, na, start, 0.35, wrap=True)
    out += 0.30 * normalize(perc, 1.0)

    # --- Melody (santoor). Notes as (beat position, swara, length in beats).
    phrase_a = [(0, "S", 1), (1, "G", 0.5), (1.5, "m", 0.5), (2, "P", 1), (3, "D", 0.5), (3.5, "P", 0.5),
                (4, "G", 1), (5, "m", 0.5), (5.5, "G", 0.5), (6, "R", 1.5), (7.5, "S", 0.5)]
    phrase_b = [(0, "P", 0.5), (0.5, "D", 0.5), (1, "N", 0.5), (1.5, "S'", 0.5), (2, "R'", 1), (3, "S'", 1),
                (4, "n", 0.5), (4.5, "D", 0.5), (5, "P", 0.5), (5.5, "m", 0.5), (6, "G", 1), (7, "R", 0.5), (7.5, "S", 0.5)]
    melody = np.zeros(total)
    song = [(0, phrase_a), (8, phrase_b), (16, phrase_a), (24, phrase_b)]
    for offset, phrase in song:
        for pos, sw, ln in phrase:
            f = sa * 2 * ratio[sw]  # one octave up
            start = int((offset + pos) * beat * SR)
            s = pluck(f, max(0.5, ln * beat * 2.2), bright=0.75, damp=0.9975)
            mix_into(melody, s, start, 1.0, wrap=True)
            # santoor double-strike tremolo on long notes
            if ln >= 1:
                mix_into(melody, pluck(f, ln * beat * 1.6, bright=0.7, damp=0.997), start + int(beat * 0.5 * SR), 0.45, wrap=True)
    out += 0.34 * normalize(melody, 1.0)

    # --- Gentle shaker on off-beats (ghungroo feel)
    shaker = np.zeros(total)
    g = highpass(rng.uniform(-1, 1, int(0.06 * SR)), 6000) * np.exp(-t_axis(0.06) / 0.015)
    for k in range(bars * 8):
        start = int((k * 0.5 + 0.25) * beat * SR)
        mix_into(shaker, g, start, 0.6 if k % 2 else 0.35, wrap=True)
    out += 0.08 * normalize(shaker, 1.0)

    out = highpass(out, 40)
    # soft clip for warmth, then normalize low so music sits under SFX
    out = np.tanh(out * 1.3)
    return normalize(out, 0.7)


def main():
    os.makedirs(OUT, exist_ok=True)
    sfx = {
        "sfx_tap.wav": sfx_tap,
        "sfx_belt_load.wav": sfx_belt_load,
        "sfx_pickup.wav": sfx_pickup,
        "sfx_shop_done.wav": sfx_shop_done,
        "sfx_win.wav": sfx_win,
        "sfx_jam.wav": sfx_jam,
        "sfx_ui_click.wav": sfx_ui_click,
        "sfx_unlock.wav": sfx_unlock,
        "sfx_ui_popup.wav": sfx_ui_popup,
        "sfx_star.wav": sfx_star,
        "sfx_coin.wav": sfx_coin,
        "sfx_purchase.wav": sfx_purchase,
        "sfx_locked.wav": sfx_locked,
    }
    for name, fn in sfx.items():
        print("writing", write_wav(name, fn()))

    tmp = write_wav("_music_tmp.wav", music_loop())
    ogg = os.path.join(OUT, "music_bazaar_loop.ogg")
    subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", tmp, "-c:a", "libvorbis", "-q:a", "4", ogg], check=True)
    os.remove(tmp)
    print("writing", ogg)


if __name__ == "__main__":
    main()
